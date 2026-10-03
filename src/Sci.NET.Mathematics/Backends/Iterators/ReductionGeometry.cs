// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Sci.NET.Mathematics.Comparison;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Iterators;

/// <summary>
/// Describes the geometry of a reduction operation.
/// </summary>
public readonly struct ReductionGeometry : IValueEquatable<ReductionGeometry>
{
    private ReductionGeometry(
        ReductionPattern pattern,
        long outerCount,
        long innerCount,
        long totalElements,
        int[] inputShape,
        long[]? tensorStrides = null,
        int[]? resultToTensorDim = null,
        int[]? reduceAxisDims = null,
        long[]? reduceAxisStrides = null,
        long outerStride = 0)
    {
        Pattern = pattern;
        OuterCount = outerCount;
        InnerCount = innerCount;
        TotalElements = totalElements;
        TensorStrides = tensorStrides ?? [];
        ResultToTensorDim = resultToTensorDim ?? [];
        ReduceAxisDims = reduceAxisDims ?? [];
        ReduceAxisStrides = reduceAxisStrides ?? [];
        OuterStride = outerStride;
        InputShape = inputShape;
    }

    /// <summary>
    /// Gets the reduction pattern.
    /// </summary>
    public ReductionPattern Pattern { get; }

    /// <summary>
    /// Gets the number of outer elements.
    /// </summary>
    public long OuterCount { get; }

    /// <summary>
    /// Gets the number of inner elements.
    /// </summary>
    public long InnerCount { get; }

    /// <summary>
    /// Gets the total number of elements.
    /// </summary>
    public long TotalElements { get; }

    /// <summary>
    /// Gets the tensor strides.
    /// </summary>
    public long[] TensorStrides { get; }

    /// <summary>
    /// Gets the mapping from result dimensions to tensor dimensions.
    /// </summary>
    public int[] ResultToTensorDim { get; }

    /// <summary>
    /// Gets the dimensions of the reduction axes.
    /// </summary>
    public int[] ReduceAxisDims { get; }

    /// <summary>
    /// Gets the strides of the reduction axes.
    /// </summary>
    public long[] ReduceAxisStrides { get; }

    /// <summary>
    /// Gets the outer stride (used for contiguous outer reductions).
    /// </summary>
    public long OuterStride { get; }

    /// <summary>
    /// Gets the input shape for which this geometry was computed.
    /// </summary>
    public int[] InputShape { get; }

    /// <inheritdoc />
    public static bool operator ==(ReductionGeometry left, ReductionGeometry right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(ReductionGeometry left, ReductionGeometry right) => !left.Equals(right);

    /// <summary>
    /// Computes the <see cref="ReductionGeometry"/> for the specified input and output shapes and reduction axes.
    /// </summary>
    /// <param name="inputShape">The input shape.</param>
    /// <param name="outputShape">The output shape.</param>
    /// <param name="axes">The reduction axes.</param>
    /// <returns>The computed <see cref="ReductionGeometry"/>.</returns>
    public static ReductionGeometry Compute(Shape inputShape, Shape outputShape, ReadOnlySpan<int> axes)
    {
        var inputRank = inputShape.Rank;
        var totalElements = inputShape.ElementCount;

        var innerCount = 1;
        foreach (var axis in axes)
        {
            innerCount *= inputShape[axis];
        }

        var outerCount = outputShape.ElementCount;

        if (axes.Length == inputRank || outerCount == 1)
        {
            return new ReductionGeometry(
                ReductionPattern.FullReduction,
                outerCount: 1,
                innerCount: totalElements,
                totalElements: totalElements,
                inputShape: inputShape.Dimensions);
        }

        if (IsContiguousInner(inputRank, axes))
        {
            return new ReductionGeometry(
                ReductionPattern.ContiguousInner,
                outerCount: outerCount,
                innerCount: innerCount,
                totalElements: totalElements,
                inputShape: inputShape.Dimensions);
        }

        if (IsContiguousOuter(axes))
        {
            var outerStride = 1;
            for (var i = axes.Length; i < inputRank; i++)
            {
                outerStride *= inputShape[i];
            }

            return new ReductionGeometry(
                ReductionPattern.ContiguousOuter,
                outerCount: outerCount,
                innerCount: innerCount,
                totalElements: totalElements,
                inputShape: inputShape.Dimensions,
                outerStride: outerStride);
        }

        return ComputeStridedGeometry(
            inputShape,
            outputShape,
            axes,
            outerCount,
            innerCount,
            totalElements);
    }

    /// <summary>
    /// Determines whether the specified <see cref="ReductionGeometry"/> is equal to the current <see cref="ReductionGeometry"/>.
    /// </summary>
    /// <param name="other">The <see cref="ReductionGeometry"/> to compare with the current <see cref="ReductionGeometry"/>.</param>
    /// <returns>true if the specified <see cref="ReductionGeometry"/> is equal to the current <see cref="ReductionGeometry"/>; otherwise, false.</returns>
    public bool Equals(ReductionGeometry other)
    {
        return Pattern == other.Pattern && OuterCount == other.OuterCount && InnerCount == other.InnerCount &&
               TotalElements == other.TotalElements && TensorStrides.SequenceEqual(other.TensorStrides) &&
               ResultToTensorDim.SequenceEqual(other.ResultToTensorDim) &&
               ReduceAxisDims.SequenceEqual(other.ReduceAxisDims) &&
               ReduceAxisStrides.SequenceEqual(other.ReduceAxisStrides) && OuterStride == other.OuterStride;
    }

    /// <inheritdoc cref="IEquatable{T}.Equals" />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is ReductionGeometry other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc cref="IValueEquatable{T}.GetHashCode" />
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(Pattern);
        hashCode.Add(OuterCount);
        hashCode.Add(InnerCount);
        hashCode.Add(TotalElements);
        foreach (var stride in TensorStrides)
        {
            hashCode.Add(stride);
        }

        foreach (var dim in ResultToTensorDim)
        {
            hashCode.Add(dim);
        }

        foreach (var dim in ReduceAxisDims)
        {
            hashCode.Add(dim);
        }

        foreach (var stride in ReduceAxisStrides)
        {
            hashCode.Add(stride);
        }

        hashCode.Add(OuterStride);
        return hashCode.ToHashCode();
    }

    private static bool IsContiguousInner(int rank, ReadOnlySpan<int> axes)
    {
        if (axes.Length == 0)
        {
            return false;
        }

        Span<int> sorted = stackalloc int[axes.Length];
        axes.CopyTo(sorted);
        sorted.Sort();

        var expectedAxis = rank - axes.Length;
        foreach (var axis in sorted)
        {
            if (axis != expectedAxis++)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsContiguousOuter(ReadOnlySpan<int> axes)
    {
        if (axes.Length == 0)
        {
            return false;
        }

        Span<int> sorted = stackalloc int[axes.Length];
        axes.CopyTo(sorted);
        sorted.Sort();

        for (var i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] != i)
            {
                return false;
            }
        }

        return true;
    }

    private static ReductionGeometry ComputeStridedGeometry(
        Shape inputShape,
        Shape outputShape,
        ReadOnlySpan<int> axes,
        long outerCount,
        long innerCount,
        long totalElements)
    {
        var inputRank = inputShape.Rank;
        var outputRank = outputShape.Rank;

        var axisSet = new HashSet<int>();
        foreach (var axis in axes)
        {
            _ = axisSet.Add(axis);
        }

        var tensorStrides = new long[inputRank];
        var stride = 1;
        for (var i = inputRank - 1; i >= 0; i--)
        {
            tensorStrides[i] = stride;
            stride *= inputShape[i];
        }

        var resultToTensorDim = new int[outputRank];
        var resultDim = 0;
        for (var tensorDim = 0; tensorDim < inputRank; tensorDim++)
        {
            if (!axisSet.Contains(tensorDim))
            {
                resultToTensorDim[resultDim++] = tensorDim;
            }
        }

        var reduceAxisDims = new int[axes.Length];
        var reduceAxisStrides = new long[axes.Length];
        for (var i = 0; i < axes.Length; i++)
        {
            reduceAxisDims[i] = inputShape[axes[i]];
            reduceAxisStrides[i] = tensorStrides[axes[i]];
        }

        return new ReductionGeometry(
            ReductionPattern.Strided,
            outerCount: outerCount,
            innerCount: innerCount,
            totalElements: totalElements,
            inputShape: inputShape.Dimensions,
            tensorStrides: tensorStrides,
            resultToTensorDim: resultToTensorDim,
            reduceAxisDims: reduceAxisDims,
            reduceAxisStrides: reduceAxisStrides);
    }
}
