// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A dedicated, fixed-size pool of worker threads that executes balanced fork-join parallel regions using an
/// OpenMP/MKL-style barrier. rather than a work queue. On each region the calling thread  publishes a single
/// shared descriptor, wakes the participating workers via a generation counter, runs one slice itself, and
/// blocks until every worker has completed.
/// </summary>
[PublicAPI]
public sealed unsafe class ParallelExecutorThreadPool : IDisposable
{
    private const int DefaultBlockTimeSpins = 10_000;

    private static readonly TimeSpan WorkerJoinTimeout = TimeSpan.FromSeconds(5);

    private readonly Lock _regionGate = new();
    private readonly int _blockTimeSpins;
    private readonly bool _pinThreads;

    private PaddedLong _generation;
    private PaddedInt _remaining;
    private WorkDescriptor _descriptor;

    private ParallelExecutorThreadPoolThread[] _threads;
    private PaddedInt[] _parked;
    private ManualResetEventSlim[] _wake;
    private Exception?[] _exceptions;
    private volatile bool _shutdown;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelExecutorThreadPool"/> class and starts
    /// its worker threads.
    /// </summary>
    /// <param name="numThreads">The number of worker threads to start.</param>
    /// <param name="priority">The priority assigned to the worker threads.</param>
    /// <param name="pinThreads">When true, attempts to pin threads to a core.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numThreads"/> is not positive.</exception>
    public ParallelExecutorThreadPool(int numThreads, ThreadPriority priority = ThreadPriority.Normal, bool pinThreads = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);

        _blockTimeSpins = DefaultBlockTimeSpins;
        _pinThreads = pinThreads;
        _disposed = false;

        StartWorkers(numThreads, priority);

        ParallelExecutorEventSource.Log.ThreadPoolStarted(numThreads);
    }

    /// <summary>
    /// Gets the worker threads owned by the pool.
    /// </summary>
    public IReadOnlyList<Thread?> Threads => [.. _threads.Select(static x => x.GetUnderlyingThread())];

    [field: ThreadStatic]
    internal static bool IsRegionActive { get; private set; }

    /// <summary>
    /// Gets the maximum number of participants in a region, including the calling (master) thread.
    /// </summary>
    internal int MaxParticipants => _threads.Length + 1;

    /// <summary>
    /// Replaces the worker threads with the given number of threads, each of which have the given priority.
    /// </summary>
    /// <param name="numThreads">The number of workers to use.</param>
    /// <param name="priority">The priority of the worker threads.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numThreads"/> is not positive.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="ParallelExecutorThreadPool"/> has been disposed.</exception>
    /// <remarks>
    /// This method blocks the current thread until the running workers have exited, then starts the
    /// replacement set.
    /// </remarks>
    public void ReplaceWorkerThreads(int numThreads, ThreadPriority priority)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_regionGate)
        {
            ParallelExecutorEventSource.Log.ThreadPoolStopping();

            SignalShutdownAndJoin(null);
            DisposeGates();

            StartWorkers(numThreads, priority);

            ParallelExecutorEventSource.Log.ThreadPoolStarted(numThreads);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ParallelExecutorEventSource.Log.ThreadPoolStopping();

        SignalShutdownAndJoin(WorkerJoinTimeout);
        DisposeGates();
    }

    internal void Invoke(
        int participants,
        delegate*<object?, long, long, long, long, void> body,
        object? state,
        long from,
        long chunk,
        long remainder)
    {
        if (participants <= 1)
        {
            body(state, 0, from, chunk, remainder);
            return;
        }

        List<Exception>? faults = null;

        lock (_regionGate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (participants > MaxParticipants)
            {
                participants = MaxParticipants;
            }

            var activeWorkers = participants - 1;

            for (var i = 0; i <= activeWorkers; i++)
            {
                _exceptions[i] = null;
            }

            _descriptor = new WorkDescriptor
            {
                Body = body,
                State = state,
                From = from,
                Chunk = chunk,
                Remainder = remainder,
                ActiveWorkers = activeWorkers
            };

            Volatile.Write(ref _remaining.Value, activeWorkers);

            var generation = _generation.Value + 1;
            Volatile.Write(ref _generation.Value, generation);

            ParallelExecutorEventSource.Log.BatchEnqueued(activeWorkers);

            Interlocked.MemoryBarrier();

            for (var i = 0; i < activeWorkers; i++)
            {
                if (Volatile.Read(ref _parked[i].Value) == 1)
                {
                    _wake[i].Set();
                }
            }

            var wasRegionActive = IsRegionActive;
            IsRegionActive = true;

            try
            {
                body(state, activeWorkers, from, chunk, remainder);
            }
            catch (Exception ex)
            {
                _exceptions[activeWorkers] = ex;
                ParallelExecutorEventSource.Log.TaskFaulted(ex.GetType().Name);
            }
            finally
            {
                IsRegionActive = wasRegionActive;

                var spinner = default(SpinWait);
                while (Volatile.Read(ref _remaining.Value) != 0)
                {
                    spinner.SpinOnce(-1);
                }
            }

            for (var i = 0; i <= activeWorkers; i++)
            {
                if (_exceptions[i] is { } fault)
                {
                    (faults ??= new List<Exception>()).Add(fault);
                    _exceptions[i] = null;
                }
            }
        }

        if (faults is not null)
        {
            throw new AggregateException(faults);
        }
    }

    /// <summary>
    /// The body of a worker thread: it waits for each region on the generation counter (spinning, then
    /// parking) and runs its slice when its index participates.
    /// </summary>
    /// <param name="workerIndex">The pool-local index of this worker.</param>
    internal void RunWorker(int workerIndex)
    {
        ParallelExecutorThreadPoolThread.MarkWorkerThread();

        if (_pinThreads)
        {
            ProcessorAffinity.TrySetForCurrentThread(workerIndex % Environment.ProcessorCount);
        }

        ParallelExecutorEventSource.Log.WorkerThreadStarted(workerIndex);

        var localGeneration = _generation.Value;
        var spinner = default(SpinWait);

        while (true)
        {
            while (true)
            {
                if (_shutdown)
                {
                    ParallelExecutorEventSource.Log.WorkerThreadStopped(workerIndex);
                    return;
                }

                var generation = Volatile.Read(ref _generation.Value);
                if (generation != localGeneration)
                {
                    localGeneration = generation;
                    break;
                }

                if (spinner.Count < _blockTimeSpins)
                {
                    spinner.SpinOnce(-1);
                }
                else
                {
                    ParkUntilNextRegion(workerIndex, localGeneration);
                    spinner = default;
                }
            }

            spinner = default;

            var descriptor = _descriptor;

            if (workerIndex < descriptor.ActiveWorkers)
            {
                try
                {
                    descriptor.Body(descriptor.State, workerIndex, descriptor.From, descriptor.Chunk, descriptor.Remainder);
                }
                catch (Exception ex)
                {
                    _exceptions[workerIndex] = ex;
                    ParallelExecutorEventSource.Log.TaskFaulted(ex.GetType().Name);
                }

                _ = Interlocked.Decrement(ref _remaining.Value);
            }
        }
    }

    [MemberNotNull(nameof(_threads), nameof(_parked), nameof(_wake), nameof(_exceptions))]
    private void StartWorkers(int numThreads, ThreadPriority priority)
    {
        _shutdown = false;
        _threads = new ParallelExecutorThreadPoolThread[numThreads];
        _parked = new PaddedInt[numThreads];
        _wake = new ManualResetEventSlim[numThreads];
        _exceptions = new Exception?[numThreads + 1];

        for (var i = 0; i < numThreads; i++)
        {
            _wake[i] = new ManualResetEventSlim(false);

            var thread = new ParallelExecutorThreadPoolThread(this)
            {
                ThreadIdx = i,
                Priority = priority
            };

            _threads[i] = thread;
            thread.Start();
        }
    }

    private void SignalShutdownAndJoin(TimeSpan? timeout)
    {
        _shutdown = true;

        Volatile.Write(ref _generation.Value, _generation.Value + 1);
        Interlocked.MemoryBarrier();

        foreach (var gate in _wake)
        {
            gate.Set();
        }

        foreach (var thread in _threads)
        {
            if (!thread.Join(timeout))
            {
                ParallelExecutorEventSource.Log.WorkerThreadJoinTimedOut(thread.ThreadIdx);
            }
        }
    }

    private void DisposeGates()
    {
        foreach (var gate in _wake)
        {
            gate.Dispose();
        }
    }

    private void ParkUntilNextRegion(int workerIndex, long localGeneration)
    {
        var gate = _wake[workerIndex];
        gate.Reset();

        Volatile.Write(ref _parked[workerIndex].Value, 1);

        Interlocked.MemoryBarrier();

        if (Volatile.Read(ref _generation.Value) != localGeneration || _shutdown)
        {
            Volatile.Write(ref _parked[workerIndex].Value, 0);
            return;
        }

        gate.Wait();
        Volatile.Write(ref _parked[workerIndex].Value, 0);
    }

    private struct WorkDescriptor
    {
        public delegate*<object?, long, long, long, long, void> Body;
        public object? State;
        public long From;
        public long Chunk;
        public long Remainder;
        public int ActiveWorkers;
    }

    [StructLayout(LayoutKind.Explicit, Size = 128)]
    private struct PaddedLong
    {
        [FieldOffset(64)]
        public long Value;
    }

    [StructLayout(LayoutKind.Explicit, Size = 128)]
    private struct PaddedInt
    {
        [FieldOffset(64)]
        public int Value;
    }
}
