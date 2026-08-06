// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Attributes;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Iterators;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Intrinsics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal class ManagedBinaryArithmeticOperationIterator<TOp, TNumber>
    where TOp : IBinaryOperation<TNumber>, IBinaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    private readonly DimRange[] _dimRanges;
    private readonly unsafe TNumber* _leftPtr;
    private readonly unsafe TNumber* _rightPtr;
    private readonly unsafe TNumber* _resultPtr;
    private readonly ICpuComputeDevice _device;

    [AssumesValidDevice]
    public unsafe ManagedBinaryArithmeticOperationIterator(ITensor<TNumber> left, ITensor<TNumber> right, ITensor<TNumber> result)
    {
        _device = (ICpuComputeDevice)result.Device;
        _dimRanges = BuildDimRanges(left, right, result);
        _leftPtr = left.Memory.ToPointer();
        _rightPtr = right.Memory.ToPointer();
        _resultPtr = result.Memory.ToPointer();
    }

    /// <summary>
    /// Iterates over the tensors and applies the given action to each element.
    /// </summary>
    [SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "By design")]
    public unsafe void Apply()
    {
        var rank = _dimRanges.Length;

        if (rank == 0)
        {
            _resultPtr[0] = TOp.ApplyScalar(_leftPtr[0], _rightPtr[0]);
        }
        else if (rank == 1)
        {
            if (_device.IsAvx2Supported() && TOp.HasAvx2Implementation())
            {
                switch (TNumber.Zero)
                {
                    case float:
                        Apply1dAvx2((float*)_leftPtr, (float*)_rightPtr, (float*)_resultPtr);
                        return;
                    case double:
                        Apply1dAvx2((double*)_leftPtr, (double*)_rightPtr, (double*)_resultPtr);
                        return;
                }
            }

            Apply1dScalar(_leftPtr, _rightPtr, _resultPtr);
        }
        else if (rank == 2)
        {
            if (_device.IsAvx2Supported() && TOp.HasAvx2Implementation())
            {
                switch (TNumber.Zero)
                {
                    case float:
                        Apply2dAvx2((float*)_leftPtr, (float*)_rightPtr, (float*)_resultPtr);
                        return;
                    case double:
                        Apply2dAvx2((double*)_leftPtr, (double*)_rightPtr, (double*)_resultPtr);
                        return;
                }
            }

            Apply2dScalar(_leftPtr, _rightPtr, _resultPtr);
        }
        else
        {
            if (_device.IsAvx2Supported() && TOp.HasAvx2Implementation())
            {
                switch (TNumber.Zero)
                {
                    case float:
                        ApplyNdAvx2((float*)_leftPtr, (float*)_rightPtr, (float*)_resultPtr);
                        return;
                    case double:
                        ApplyNdAvx2((double*)_leftPtr, (double*)_rightPtr, (double*)_resultPtr);
                        return;
                }
            }

            ApplyNdScalar(_leftPtr, _rightPtr, _resultPtr);
        }
    }

    private static DimRange[] BuildDimRanges(ITensor<TNumber> left, ITensor<TNumber> right, ITensor<TNumber> result)
    {
        var outRank = result.Shape.Rank;

        var leftDimsPadded = PadShape(left.Shape.Dimensions, outRank);
        var leftStridesPadded = PadStrides(left.Shape.Strides, outRank);
        var rightDimsPadded = PadShape(right.Shape.Dimensions, outRank);
        var rightStridesPadded = PadStrides(right.Shape.Strides, outRank);
        var outDimsPadded = PadShape(result.Shape.Dimensions, outRank);
        var outputStridesPadded = PadStrides(result.Shape.Strides, outRank);

        var merged = new List<DimRange>();

        var dim = outRank - 1;

        while (dim >= 0)
        {
            var size = outDimsPadded[dim];

            var leftStride = leftDimsPadded[dim] == 1 ? 0 : leftStridesPadded[dim];
            var rightStride = rightDimsPadded[dim] == 1 ? 0 : rightStridesPadded[dim];
            var outputStride = outputStridesPadded[dim];

            var mergedSize = size;
            var moveDim = dim - 1;

            while (moveDim >= 0)
            {
                var nextSize = outDimsPadded[moveDim];
                var nextLeftDim = leftDimsPadded[moveDim];
                var nextRightDim = rightDimsPadded[moveDim];

                var nextLeftStride = nextLeftDim == 1 ? 0 : leftStridesPadded[moveDim];
                var nextRightStride = nextRightDim == 1 ? 0 : rightStridesPadded[moveDim];
                var nextOutStride = outputStridesPadded[moveDim];

                var aOk = leftStride == nextLeftStride * nextSize || leftStride == 0 || nextLeftStride == 0;
                var bOk = rightStride == nextRightStride * nextSize || rightStride == 0 || nextRightStride == 0;
                var outOk = outputStride == nextOutStride * nextSize;

                if (!aOk || !bOk || !outOk)
                {
                    break;
                }

                mergedSize *= nextSize;
                leftStride = leftStride != 0 ? leftStride : nextLeftStride;
                rightStride = rightStride != 0 ? rightStride : nextRightStride;

                moveDim--;
            }

            merged.Add(
                new DimRange
                {
                    Extent = mergedSize,
                    StrideLeft = leftStride,
                    StrideRight = rightStride,
                    StrideResult = outputStride
                });

            dim = moveDim;
        }

        merged.Reverse();

        return merged.ToArray();
    }

    private static int[] PadShape(int[] dims, int rankWanted)
    {
        var diff = rankWanted - dims.Length;
        if (diff <= 0)
        {
            return dims;
        }

        var padded = new int[rankWanted];
        for (var i = 0; i < diff; i++)
        {
            padded[i] = 1;
        }

        for (var i = 0; i < dims.Length; i++)
        {
            padded[diff + i] = dims[i];
        }

        return padded;
    }

    private static long[] PadStrides(long[] strides, int rankWanted)
    {
        var diff = rankWanted - strides.Length;
        if (diff <= 0)
        {
            return strides;
        }

        var padded = new long[rankWanted];
        for (var i = 0; i < diff; i++)
        {
            padded[i] = 0;
        }

        for (var i = 0; i < strides.Length; i++)
        {
            padded[diff + i] = strides[i];
        }

        return padded;
    }

    private unsafe void Apply1dScalar(TNumber* leftPtr, TNumber* rightPtr, TNumber* resultPtr)
    {
        var d0 = _dimRanges[0];
        var extent = d0.Extent;
        var sL = d0.StrideLeft;
        var sR = d0.StrideRight;
        var sO = d0.StrideResult;

        _ = Parallel.For(
            0,
            extent,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            i =>
            {
                var offsetLeft = i * sL;
                var offsetRight = i * sR;
                var offsetResult = i * sO;

                resultPtr[offsetResult] = TOp.ApplyScalar(leftPtr[offsetLeft], rightPtr[offsetRight]);
            });
    }

    private unsafe void Apply1dAvx2(float* leftPtr, float* rightPtr, float* resultPtr)
    {
        var d0 = _dimRanges[0];
        var extent = d0.Extent;
        var sL = d0.StrideLeft;
        var sR = d0.StrideRight;
        var sO = d0.StrideResult;

        if (sL == 1 && sR == 1 && sO == 1)
        {
            var rangePartitioner = Partitioner.Create(0L, extent, Math.Max(IntrinsicsHelper.AvxVectorSizeFp32 * 16, 4096));

            _ = Parallel.ForEach(
                rangePartitioner,
                range =>
                {
                    var (start, end) = range;
                    var i = start;

                    for (; i <= end - IntrinsicsHelper.AvxVectorSizeFp32; i += IntrinsicsHelper.AvxVectorSizeFp32)
                    {
                        var leftVector = Avx.LoadVector256(leftPtr + i);
                        var rightVector = Avx.LoadVector256(rightPtr + i);
                        var resultVector = TOp.ApplyAvxFp32(leftVector, rightVector);
                        resultVector.Store(resultPtr + i);
                    }

                    for (; i < end; i++)
                    {
                        resultPtr[i] = TOp.ApplyScalarFp32(leftPtr[i], rightPtr[i]);
                    }
                });
        }
        else
        {
            _ = Parallel.For(
                0,
                extent,
                new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
                i =>
                {
                    var offsetLeft = i * sL;
                    var offsetRight = i * sR;
                    var offsetResult = i * sO;

                    resultPtr[offsetResult] = TOp.ApplyScalarFp32(leftPtr[offsetLeft], rightPtr[offsetRight]);
                });
        }
    }

    private unsafe void Apply1dAvx2(double* leftPtr, double* rightPtr, double* resultPtr)
    {
        var d0 = _dimRanges[0];
        var extent = d0.Extent;
        var sL = d0.StrideLeft;
        var sR = d0.StrideRight;
        var sO = d0.StrideResult;

        if (sL == 1 && sR == 1 && sO == 1)
        {
            var rangePartitioner = Partitioner.Create(0L, extent, Math.Max(IntrinsicsHelper.AvxVectorSizeFp64 * 16, 4096));

            _ = Parallel.ForEach(
                rangePartitioner,
                range =>
                {
                    var (start, end) = range;
                    var i = start;

                    for (; i <= end - IntrinsicsHelper.AvxVectorSizeFp64; i += IntrinsicsHelper.AvxVectorSizeFp64)
                    {
                        var leftVector = Avx.LoadVector256(leftPtr + i);
                        var rightVector = Avx.LoadVector256(rightPtr + i);
                        var resultVector = TOp.ApplyAvxFp64(leftVector, rightVector);
                        resultVector.Store(resultPtr + i);
                    }

                    for (; i < end; i++)
                    {
                        resultPtr[i] = TOp.ApplyScalarFp64(leftPtr[i], rightPtr[i]);
                    }
                });
        }
        else
        {
            _ = Parallel.For(
                0,
                extent,
                new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
                i =>
                {
                    var offsetLeft = i * sL;
                    var offsetRight = i * sR;
                    var offsetResult = i * sO;

                    resultPtr[offsetResult] = TOp.ApplyScalarFp64(leftPtr[offsetLeft], rightPtr[offsetRight]);
                });
        }
    }

    private unsafe void Apply2dScalar(TNumber* leftPtr, TNumber* rightPtr, TNumber* resultPtr)
    {
        var dim0 = _dimRanges[0];
        var dim1 = _dimRanges[1];

        var extent0 = dim0.Extent;
        var extent1 = dim1.Extent;

        _ = Parallel.For(
            0,
            extent0,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            i =>
            {
                var baseLeft = i * dim0.StrideLeft;
                var baseRight = i * dim0.StrideRight;
                var baseOut = i * dim0.StrideResult;

                for (long j = 0; j < extent1; j++)
                {
                    var offLeft = baseLeft + (j * dim1.StrideLeft);
                    var offRight = baseRight + (j * dim1.StrideRight);
                    var offOut = baseOut + (j * dim1.StrideResult);

                    resultPtr[offOut] = TOp.ApplyScalar(leftPtr[offLeft], rightPtr[offRight]);
                }
            });
    }

    private unsafe void Apply2dAvx2(float* leftPtr, float* rightPtr, float* resultPtr)
    {
        var dim0 = _dimRanges[0];
        var dim1 = _dimRanges[1];

        var extent0 = dim0.Extent;
        var extent1 = dim1.Extent;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(float);

        _ = Parallel.For(
            0,
            extent0,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            i =>
            {
                var baseLeft = i * dim0.StrideLeft;
                var baseRight = i * dim0.StrideRight;
                var baseResult = i * dim0.StrideResult;

                var j = 0L;
                for (; j < extent1 - IntrinsicsHelper.AvxVectorSizeFp32; j += IntrinsicsHelper.AvxVectorSizeFp32)
                {
                    var offsetLeft = baseLeft + (j * dim1.StrideLeft);
                    var offsetRight = baseRight + (j * dim1.StrideRight);
                    var offsetResult = baseResult + (j * dim1.StrideResult);

                    Sse.Prefetch0(leftPtr + offsetLeft + i + prefetchVectorCount);
                    Sse.Prefetch0(rightPtr + offsetRight + i + prefetchVectorCount);
                    Sse.PrefetchNonTemporal(resultPtr + offsetRight + i + prefetchVectorCount);

                    var leftVector = Avx.LoadVector256(leftPtr + offsetLeft);
                    var rightVector = Avx.LoadVector256(rightPtr + offsetRight);
                    var result = TOp.ApplyAvxFp32(leftVector, rightVector);

                    Avx.Store(resultPtr + offsetResult, result);
                }

                for (; j < extent1; j++)
                {
                    var offsetLeft = baseLeft + (j * dim1.StrideLeft);
                    var offsetRight = baseRight + (j * dim1.StrideRight);
                    var offsetResult = baseResult + (j * dim1.StrideResult);

                    resultPtr[offsetResult] = TOp.ApplyScalarFp32(leftPtr[offsetLeft], rightPtr[offsetRight]);
                }
            });
    }

    private unsafe void Apply2dAvx2(double* leftPtr, double* rightPtr, double* resultPtr)
    {
        var dim0 = _dimRanges[0];
        var dim1 = _dimRanges[1];

        var extent0 = dim0.Extent;
        var extent1 = dim1.Extent;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        _ = Parallel.For(
            0,
            extent0,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            i =>
            {
                var baseLeft = i * dim0.StrideLeft;
                var baseRight = i * dim0.StrideRight;
                var baseResult = i * dim0.StrideResult;

                var j = 0L;
                for (; j < extent1 - IntrinsicsHelper.AvxVectorSizeFp64; j += IntrinsicsHelper.AvxVectorSizeFp64)
                {
                    var offsetLeft = baseLeft + (j * dim1.StrideLeft);
                    var offsetRight = baseRight + (j * dim1.StrideRight);
                    var offsetResult = baseResult + (j * dim1.StrideResult);

                    Sse.Prefetch0(leftPtr + offsetLeft + i + prefetchVectorCount);
                    Sse.Prefetch0(rightPtr + offsetRight + i + prefetchVectorCount);
                    Sse.PrefetchNonTemporal(resultPtr + offsetRight + i + prefetchVectorCount);

                    var leftVector = Avx.LoadVector256(leftPtr + offsetLeft);
                    var rightVector = Avx.LoadVector256(rightPtr + offsetRight);
                    var result = TOp.ApplyAvxFp64(leftVector, rightVector);

                    Avx.Store(resultPtr + offsetResult, result);
                }

                for (; j < extent1; j++)
                {
                    var offsetLeft = baseLeft + (j * dim1.StrideLeft);
                    var offsetRight = baseRight + (j * dim1.StrideRight);
                    var offsetResult = baseResult + (j * dim1.StrideResult);

                    resultPtr[offsetResult] = TOp.ApplyScalarFp64(leftPtr[offsetLeft], rightPtr[offsetRight]);
                }
            });
    }

    private unsafe void ApplyNdScalar(TNumber* leftPtr, TNumber* rightPtr, TNumber* resultPtr)
    {
        var rank = _dimRanges.Length;
        var innerDim = _dimRanges[rank - 1];
        var innerExtent = innerDim.Extent;

        long outerTotal = 1;
        for (var d = 0; d < rank - 1; d++)
        {
            outerTotal *= _dimRanges[d].Extent;
        }

        _ = Parallel.For(
            0,
            outerTotal,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            outerIdx =>
            {
                var baseLeft = 0L;
                var baseRight = 0L;
                var baseResult = 0L;

                var tmp = outerIdx;
                for (var dim = rank - 2; dim >= 0; dim--)
                {
                    var ext = _dimRanges[dim].Extent;
                    var coord = tmp % ext;
                    tmp /= ext;

                    baseLeft += coord * _dimRanges[dim].StrideLeft;
                    baseRight += coord * _dimRanges[dim].StrideRight;
                    baseResult += coord * _dimRanges[dim].StrideResult;
                }

                for (var j = 0L; j < innerExtent; j++)
                {
                    var offsetLeft = baseLeft + (j * innerDim.StrideLeft);
                    var offsetRight = baseRight + (j * innerDim.StrideRight);
                    var offsetResult = baseResult + (j * innerDim.StrideResult);

                    resultPtr[offsetResult] = TOp.ApplyScalar(leftPtr[offsetLeft], rightPtr[offsetRight]);
                }
            });
    }

    private unsafe void ApplyNdAvx2(float* leftPtr, float* rightPtr, float* resultPtr)
    {
        var rank = _dimRanges.Length;
        var innerDim = _dimRanges[rank - 1];
        var innerExtent = innerDim.Extent;

        long outerTotal = 1;
        for (var d = 0; d < rank - 1; d++)
        {
            outerTotal *= _dimRanges[d].Extent;
        }

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(float);

        _ = Parallel.For(
            0,
            outerTotal,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            outerIdx =>
            {
                var baseLeft = 0L;
                var baseRight = 0L;
                var baseResult = 0L;

                var tmp = outerIdx;
                for (var dim = rank - 2; dim >= 0; dim--)
                {
                    var ext = _dimRanges[dim].Extent;
                    var coord = tmp % ext;
                    tmp /= ext;

                    baseLeft += coord * _dimRanges[dim].StrideLeft;
                    baseRight += coord * _dimRanges[dim].StrideRight;
                    baseResult += coord * _dimRanges[dim].StrideResult;
                }

                var j = 0L;
                for (; j < innerExtent - IntrinsicsHelper.AvxVectorSizeFp32; j += IntrinsicsHelper.AvxVectorSizeFp32)
                {
                    var offsetLeft = baseLeft + (j * innerDim.StrideLeft);
                    var offsetRight = baseRight + (j * innerDim.StrideRight);
                    var offsetResult = baseResult + (j * innerDim.StrideResult);

                    Sse.Prefetch0(leftPtr + offsetLeft + prefetchVectorCount);
                    Sse.Prefetch0(rightPtr + offsetLeft + prefetchVectorCount);
                    Sse.PrefetchNonTemporal(resultPtr + offsetLeft + prefetchVectorCount);

                    var leftVector = Avx.LoadVector256(leftPtr + offsetLeft);
                    var rightVector = Avx.LoadVector256(rightPtr + offsetRight);
                    var result = TOp.ApplyAvxFp32(leftVector, rightVector);

                    Avx.Store(resultPtr + offsetResult, result);
                }

                for (; j < innerExtent; j++)
                {
                    var offsetLeft = baseLeft + (j * innerDim.StrideLeft);
                    var offsetRight = baseRight + (j * innerDim.StrideRight);
                    var offsetResult = baseResult + (j * innerDim.StrideResult);

                    resultPtr[offsetResult] = TOp.ApplyScalarFp32(leftPtr[offsetLeft], rightPtr[offsetRight]);
                }
            });
    }

    private unsafe void ApplyNdAvx2(double* leftPtr, double* rightPtr, double* resultPtr)
    {
        var rank = _dimRanges.Length;
        var innerDim = _dimRanges[rank - 1];
        var innerExtent = innerDim.Extent;

        long outerTotal = 1;
        for (var d = 0; d < rank - 1; d++)
        {
            outerTotal *= _dimRanges[d].Extent;
        }

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        _ = Parallel.For(
            0,
            outerTotal,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            outerIdx =>
            {
                var baseLeft = 0L;
                var baseRight = 0L;
                var baseResult = 0L;

                var tmp = outerIdx;
                for (var dim = rank - 2; dim >= 0; dim--)
                {
                    var ext = _dimRanges[dim].Extent;
                    var coord = tmp % ext;
                    tmp /= ext;

                    baseLeft += coord * _dimRanges[dim].StrideLeft;
                    baseRight += coord * _dimRanges[dim].StrideRight;
                    baseResult += coord * _dimRanges[dim].StrideResult;
                }

                var j = 0L;
                for (; j < innerExtent - IntrinsicsHelper.AvxVectorSizeFp64; j += IntrinsicsHelper.AvxVectorSizeFp64)
                {
                    var offsetLeft = baseLeft + (j * innerDim.StrideLeft);
                    var offsetRight = baseRight + (j * innerDim.StrideRight);
                    var offsetResult = baseResult + (j * innerDim.StrideResult);

                    Sse.Prefetch0(leftPtr + offsetLeft + prefetchVectorCount);
                    Sse.Prefetch0(rightPtr + offsetLeft + prefetchVectorCount);
                    Sse.PrefetchNonTemporal(resultPtr + offsetLeft + prefetchVectorCount);

                    var leftVector = Avx.LoadVector256(leftPtr + offsetLeft);
                    var rightVector = Avx.LoadVector256(rightPtr + offsetRight);
                    var result = TOp.ApplyAvxFp64(leftVector, rightVector);

                    Avx.Store(resultPtr + offsetResult, result);
                }

                for (; j < innerExtent; j++)
                {
                    var offsetLeft = baseLeft + (j * innerDim.StrideLeft);
                    var offsetRight = baseRight + (j * innerDim.StrideRight);
                    var offsetResult = baseResult + (j * innerDim.StrideResult);

                    resultPtr[offsetResult] = TOp.ApplyScalarFp64(leftPtr[offsetLeft], rightPtr[offsetRight]);
                }
            });
    }
}