// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Sci.NET.Mathematics.Backends;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed;

namespace Sci.NET.Benchmarks.Managed.Devices;

/// <inheritdoc />
public class NoAvxManagedTensorBackend : ITensorBackend
{
    static NoAvxManagedTensorBackend()
    {
        ResetToDefaults();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoAvxManagedTensorBackend"/> class.
    /// </summary>
    public NoAvxManagedTensorBackend()
    {
        Storage = new ManagedStorageKernels();
        LinearAlgebra = new ManagedLinearAlgebraKernels();
        Arithmetic = new ManagedArithmeticKernels();
        Exponential = new ManagedExponentialKernels();
        Device = new NoAvxCpuComputeDevice();
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
    /// Gets or sets the maximum degree of parallelism for operations in the managed backend.
    /// </summary>
    public static int MaxDegreeOfParallelism
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Environment.ProcessorCount);

            field = value;
        }
    }

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
    /// Gets or sets the threshold for parallelization in terms of number of elements.
    /// </summary>
    public static int ParallelizationThreshold
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
    /// Gets the singleton instance of the <see cref="ManagedTensorBackend"/>.
    /// </summary>
    public static ITensorBackend Instance { get; } = new ManagedTensorBackend();

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
        ParallelizationThreshold = 100_000;
        ParallelizationTileThreshold = 2;
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
        where TNumber : unmanaged, INumber<TNumber>
    {
        var maxUsefulThreads = Math.Max(1, elementCount * Unsafe.SizeOf<TNumber>() / MinBytesPerThread);

        return (int)Math.Min(maxUsefulThreads, MaxDegreeOfParallelism);
    }
}