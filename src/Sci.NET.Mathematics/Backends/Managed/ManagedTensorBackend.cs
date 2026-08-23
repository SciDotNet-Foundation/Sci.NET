// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.Backends.Managed;

/// <summary>
/// An implementation of <see cref="ITensorBackend"/> for the managed backend.
/// </summary>
[PublicAPI]
public sealed class ManagedTensorBackend : ITensorBackend
{
    private static ParallelExecutorThreadPool _threadPool = null!;

    static ManagedTensorBackend()
    {
        ResetToDefaults();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedTensorBackend"/> class.
    /// </summary>
    public ManagedTensorBackend()
    {
        Storage = new ManagedStorageKernels();
        LinearAlgebra = new ManagedLinearAlgebraKernels();
        Arithmetic = new ManagedArithmeticKernels();
        Exponential = new ManagedExponentialKernels();
        Device = new CpuComputeDevice();
        Reduction = new ManagedReductionKernels();
        Trigonometry = new ManagedTrigonometryKernels();
        Random = new ManagedRandomKernels();
        Casting = new ManagedCastingKernels();
        ActivationFunctions = new ManagedActivationFunctionKernels();
        Broadcasting = new ManagedBroadcastingKernels();
        Permutation = new ManagedPermutationKernels();
        Normalisation = new ManagedNormalisationKernels();
        EqualityOperations = new ManagedEqualityOperationKernels();
    }

    /// <summary>
    /// Gets the maximum degree of parallelism for operations in the managed backend.
    /// </summary>
    public static int MaxDegreeOfParallelism { get; private set; }

    /// <summary>
    /// Gets or sets the minimum number of bytes processed per thread in parallel operations.
    /// </summary>
    public static int MinBytesPerThread
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the threshold for parallelization in terms of number of tiles.
    /// </summary>
    public static int ParallelizationTileThreshold
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    }

    /// <summary>
    /// Gets the parallel executor used to run the backend's kernels.
    /// </summary>
    public static ParallelExecutor ParallelExecutor { get; private set; } = null!;

    /// <summary>
    /// Gets the singleton instance of the <see cref="ManagedTensorBackend"/>.
    /// </summary>
    public static ManagedTensorBackend Instance { get; } = new();

    /// <inheritdoc />
    public ITensorStorageKernels Storage { get; }

    /// <inheritdoc />
    public ILinearAlgebraKernels LinearAlgebra { get; }

    /// <inheritdoc />
    public IArithmeticKernels Arithmetic { get; }

    /// <inheritdoc />
    public IExponentialKernels Exponential { get; }

    /// <inheritdoc />
    public IDevice Device { get; private set; }

    /// <inheritdoc />
    public IReductionKernels Reduction { get; }

    /// <inheritdoc />
    public ITrigonometryKernels Trigonometry { get; }

    /// <inheritdoc />
    public IRandomKernels Random { get; }

    /// <inheritdoc />
    public ICastingKernels Casting { get; }

    /// <inheritdoc />
    public IActivationFunctionKernels ActivationFunctions { get; }

    /// <inheritdoc />
    public IBroadcastingKernels Broadcasting { get; }

    /// <inheritdoc />
    public IPermutationKernels Permutation { get; }

    /// <inheritdoc />
    public INormalisationKernels Normalisation { get; }

    /// <inheritdoc />
    public IEqualityOperationKernels EqualityOperations { get; }

    /// <summary>
    /// Resets the parallelization settings to their default values.
    /// </summary>
    public static void ResetToDefaults()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount;
        MinBytesPerThread = 256 * 1024; // 256 KiB per thread
        ParallelizationTileThreshold = 2;

        _threadPool = new ParallelExecutorThreadPool(MaxDegreeOfParallelism, ThreadPriority.Normal);
        ParallelExecutor = new ParallelExecutor(_threadPool);
    }

    /// <summary>
    /// Sets the options for the <see cref="ParallelExecutor"/>, including the <paramref name="numThreads"/> and <paramref name="threadPriority"/>.
    /// The <see cref="MaxDegreeOfParallelism"/> is set to the <paramref name="numThreads"/>.
    /// </summary>
    /// <param name="numThreads">The number of threads to create.</param>
    /// <param name="threadPriority">The priority of the <see cref="ParallelExecutor"/> worker threads.</param>
    public static void SetParallelExecutorOptions(int numThreads, ThreadPriority threadPriority = ThreadPriority.Normal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numThreads, Environment.ProcessorCount);

        MaxDegreeOfParallelism = numThreads;
        _threadPool.ReplaceWorkerThreads(numThreads, priority: threadPriority);
    }

    internal static int GetMaxDegreeOfParallelism(long tileCount)
    {
        if (tileCount > int.MaxValue)
        {
            return MaxDegreeOfParallelism;
        }

        return Math.Min(MaxDegreeOfParallelism, (int)tileCount);
    }

    internal static bool ShouldParallelizeForTiles(long tileCount)
    {
        return tileCount > ParallelizationTileThreshold;
    }

    internal static int GetNumThreadsByElementCount<TNumber>(long elementCount)
        where TNumber : unmanaged
    {
        var maxUsefulThreads = Math.Max(1, elementCount * Unsafe.SizeOf<TNumber>() / MinBytesPerThread);

        return (int)Math.Min(maxUsefulThreads, MaxDegreeOfParallelism);
    }

    internal static TIndex GetNumThreadsByElementCount<TIndex, TNumber>(TIndex elementCount)
        where TIndex : IBinaryInteger<TIndex>
        where TNumber : unmanaged
    {
        var maxUsefulThreads = Math.Max(1L, long.CreateChecked(elementCount) * Unsafe.SizeOf<TNumber>() / MinBytesPerThread);

        return TIndex.CreateChecked(Math.Min(maxUsefulThreads, MaxDegreeOfParallelism));
    }

    internal static int GetNumThreadsByElementCount<T1, T2>(long elementCount)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        var maxUsefulThreads = Math.Min(
            GetNumThreadsByElementCount<T1>(elementCount),
            GetNumThreadsByElementCount<T2>(elementCount));

        return Math.Min(maxUsefulThreads, MaxDegreeOfParallelism);
    }
}