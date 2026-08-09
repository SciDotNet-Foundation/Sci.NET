// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sci.NET.Mathematics.Collections;

/// <summary>
/// Implements blocking and bounding and add-pausing for collections which implement <see cref="IProducerConsumerCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The element type of the <see cref="IProducerConsumerCollection{T}"/>.</typeparam>
/// <remarks>
/// When <see cref="PauseAdding"/> is called, no items can be added to the collection until <see cref="ResumeAdding"/> is called.
/// </remarks>
[DebuggerTypeProxy(typeof(PausableBoundedBlockingCollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}, Type = {_collection}")]
public sealed class PausableBoundedBlockingCollection<T> : IDisposable, IReadOnlyCollection<T>
{
    private const int AddingIsCompletedMask = unchecked((int)0x80000000);
    private const int BlockedWaiterPollMilliseconds = 50;
    private const int ConsumerSpinIterations = 50;

    private readonly ManualResetEventSlim _pauseGate;
    private readonly IProducerConsumerCollection<T> _collection;
    private readonly CancellationTokenSource _producersCancellationTokenSource;
    private readonly CancellationTokenSource _consumersCancellationTokenSource;
    private readonly SemaphoreSlim _freeNodes;
    private readonly SemaphoreSlim _occupiedNodes;
    private CancellationTokenSource _pausedCancellationTokenSource;
    private bool _isDisposed;
    private int _currentAdders;

    /// <summary>
    /// Initializes a new instance of the <see cref="PausableBoundedBlockingCollection{T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the collection.</param>
    /// <param name="collection">The underlying collection to use.</param>
    public PausableBoundedBlockingCollection(int capacity, IProducerConsumerCollection<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, collection.Count);

        _producersCancellationTokenSource = new CancellationTokenSource();
        _consumersCancellationTokenSource = new CancellationTokenSource();
        _pausedCancellationTokenSource = new CancellationTokenSource();
        _collection = collection;
        _isDisposed = false;
        _freeNodes = new SemaphoreSlim(capacity - collection.Count, capacity);
        _occupiedNodes = new SemaphoreSlim(collection.Count, capacity);
        _pauseGate = new ManualResetEventSlim(true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PausableBoundedBlockingCollection{T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the collection.</param>
    public PausableBoundedBlockingCollection(int capacity)
        : this(capacity, new ConcurrentQueue<T>())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PausableBoundedBlockingCollection{T}"/> class.
    /// </summary>
    ~PausableBoundedBlockingCollection()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets a value indicating whether adding is marked complete AND all in-flight adds have drained.
    /// </summary>
    public bool IsAddingCompleted
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _currentAdders == AddingIsCompletedMask;
        }
    }

    /// <summary>
    /// Gets a value indicating whether adding is marked complete and the queue has been fully drained.
    /// </summary>
    public bool IsCompleted
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return IsAddingCompleted && _occupiedNodes.CurrentCount == 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether adding has been paused.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    public bool IsPaused
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return !_pauseGate.IsSet;
        }
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _occupiedNodes.CurrentCount;
        }
    }

    /// <summary>
    /// Adds the <paramref name="item"/> to the <see cref="PausableBoundedBlockingCollection{T}"/>, blocking until there is a free slot
    /// in which to add the <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The item to be added to the collection.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <remarks>
    /// This method will block until there is capacity to add the item to the collection or until adding is resumed by <see cref="ResumeAdding"/>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken"/> is canceled.</exception>
    /// <exception cref="InvalidOperationException">The underlying collection didn't accept the item.</exception>
    public void Add(T item, CancellationToken cancellationToken = default)
    {
        if (!TryAddInternal(item, cancellationToken))
        {
            throw new InvalidOperationException("Could not add the item to the collection");
        }
    }

    /// <summary>
    /// Prevents adding to the collection.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    public void PauseAdding()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _pauseGate.Reset();
        _pausedCancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Allows adding to the collection to resume.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    public void ResumeAdding()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var old = Interlocked.Exchange(ref _pausedCancellationTokenSource, new CancellationTokenSource());
        old.Dispose();
        _pauseGate.Set();
    }

    /// <summary>
    /// Provides a consuming <see cref="IEnumerable{T}"/> for items in the collection. This <see cref="IEnumerable{T}"/>
    /// will block until more data becomes available. The enumeration will finish when either <see cref="IsAddingCompleted"/> and there
    /// are no more items in the collection, or when adding is paused and there are no more items in the collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> which removes and then returns items from the collection.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken"/> is canceled.</exception>
    /// <exception cref="InvalidCastException">If the underlying <see cref="IProducerConsumerCollection{T}"/> was modified outside of this instance.</exception>
    public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken = default)
    {
        while (!IsCompleted && !ShouldExitForEmptyQueueAndPausedAddingOrDisposed())
        {
            if (TryTakeInternal(out var item, cancellationToken))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Marks the <see cref="PausableBoundedBlockingCollection{T}"/> as not accepting
    /// any more items.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PausableBoundedBlockingCollection{T}"/> has been disposed.</exception>
    public void CompleteAdding()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var spinner = default(SpinWait);
        while (true)
        {
            int observedAdders = _currentAdders;

            if ((observedAdders & AddingIsCompletedMask) != 0)
            {
                break;
            }

            if (Interlocked.CompareExchange(
                    ref _currentAdders,
                    observedAdders | AddingIsCompletedMask,
                    observedAdders) == observedAdders)
            {
                break;
            }

            spinner.SpinOnce();
        }

        _pauseGate.Set();
        _producersCancellationTokenSource.Cancel();

        var drainSpinner = default(SpinWait);
        while (_currentAdders != AddingIsCompletedMask)
        {
            drainSpinner.SpinOnce();
        }

        if (_occupiedNodes.CurrentCount == 0)
        {
            _consumersCancellationTokenSource.Cancel();
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _isDisposed = true;

            if (!_producersCancellationTokenSource.IsCancellationRequested)
            {
                _producersCancellationTokenSource.Cancel();
            }

            if (!_consumersCancellationTokenSource.IsCancellationRequested)
            {
                _consumersCancellationTokenSource.Cancel();
            }

            _pauseGate.Dispose();
            _producersCancellationTokenSource.Dispose();
            _consumersCancellationTokenSource.Dispose();
            _pausedCancellationTokenSource.Dispose();
            _freeNodes.Dispose();
            _occupiedNodes.Dispose();
        }
    }

    private bool TryAddInternal(T item, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsAddingCompleted)
        {
            throw new InvalidOperationException("The queue is complete; no further items may be added.");
        }

        if (cancellationToken.CanBeCanceled)
        {
            WaitForFreeSlotCancellable(cancellationToken);
        }
        else
        {
            WaitForFreeSlotFast();
        }

        var spinWait = default(SpinWait);
        while (true)
        {
            int adders = _currentAdders;
            if ((adders & AddingIsCompletedMask) != 0)
            {
                _ = _freeNodes.Release();
                spinWait.Reset();

                while (_currentAdders != AddingIsCompletedMask)
                {
                    spinWait.SpinOnce();
                }

                throw new InvalidOperationException("The queue was completed concurrently; the item was not added.");
            }

            if (Interlocked.CompareExchange(ref _currentAdders, adders + 1, adders) == adders)
            {
                break;
            }

            spinWait.SpinOnce(-1);
        }

        var added = false;
        try
        {
            added = _collection.TryAdd(item);
            if (!added)
            {
                throw new InvalidOperationException("The underlying collection rejected the item despite a reserved slot.");
            }
        }
        finally
        {
            _ = added ? _occupiedNodes.Release() : _freeNodes.Release();

            _ = Interlocked.Decrement(ref _currentAdders);
        }

        return true;
    }

    private bool TryTakeInternal([NotNullWhen(true)] out T? item, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        cancellationToken.ThrowIfCancellationRequested();
        item = default;

        if (IsCompleted)
        {
            return false;
        }

        var acquired = cancellationToken.CanBeCanceled
            ? WaitForOccupiedSlotCancellable(cancellationToken)
            : WaitForOccupiedSlotFast();

        if (!acquired)
        {
            return false;
        }

        var removeSucceeded = false;
        var removeFaulted = true;
        try
        {
            removeSucceeded = _collection.TryTake(out item);
            removeFaulted = false;
            if (!removeSucceeded)
            {
                throw new InvalidOperationException("The underlying collection was modified outside this instance.");
            }
        }
        finally
        {
            if (removeSucceeded)
            {
                _ = _freeNodes.Release();

                if (IsAddingCompleted && _occupiedNodes.CurrentCount == 0)
                {
                    _consumersCancellationTokenSource.Cancel();
                }
            }
            else if (removeFaulted)
            {
                _ = _occupiedNodes.Release();
            }
        }

        return removeSucceeded;
    }

    private void WaitForFreeSlotCancellable(CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _producersCancellationTokenSource.Token);

        try
        {
            _pauseGate.Wait(linked.Token);
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Adding was completed while this producer was waiting to unpause.");
        }

        try
        {
            _ = _freeNodes.Wait(Timeout.Infinite, linked.Token);
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Adding was completed while this producer was waiting for a free slot.");
        }
    }

    private void WaitForFreeSlotFast()
    {
        _pauseGate.Wait();

        while (!_freeNodes.Wait(BlockedWaiterPollMilliseconds))
        {
            if (IsAddingCompleted)
            {
                throw new InvalidOperationException("Adding was completed while this producer was waiting for a free slot.");
            }

            ObjectDisposedException.ThrowIf(_isDisposed, this);
        }
    }

    private bool WaitForOccupiedSlotCancellable(CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _consumersCancellationTokenSource.Token,
            _pausedCancellationTokenSource.Token);

        try
        {
            _ = _occupiedNodes.Wait(Timeout.Infinite, linked.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }
    }

    private bool WaitForOccupiedSlotFast()
    {
        var spinner = default(SpinWait);

        while (spinner.Count < ConsumerSpinIterations)
        {
            if (_occupiedNodes.CurrentCount > 0 && _occupiedNodes.Wait(0))
            {
                return true;
            }

            if (ShouldStopConsuming())
            {
                return false;
            }

            spinner.SpinOnce(sleep1Threshold: -1);
        }

        while (!_occupiedNodes.Wait(BlockedWaiterPollMilliseconds))
        {
            if (ShouldStopConsuming())
            {
                return false;
            }
        }

        return true;
    }

    private bool ShouldStopConsuming()
    {
        if (_isDisposed)
        {
            return true;
        }

        if (_occupiedNodes.CurrentCount > 0)
        {
            return false;
        }

        return _currentAdders == AddingIsCompletedMask || !_pauseGate.IsSet;
    }

    private bool ShouldExitForEmptyQueueAndPausedAddingOrDisposed()
    {
        if (_isDisposed)
        {
            return true;
        }

        if (!IsPaused)
        {
            return false;
        }

        return _occupiedNodes.CurrentCount == 0;
    }
}