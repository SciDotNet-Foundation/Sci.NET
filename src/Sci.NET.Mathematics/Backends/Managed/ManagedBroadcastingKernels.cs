// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed;

internal class ManagedBroadcastingKernels : IBroadcastingKernels
{
    public unsafe void Broadcast<TNumber>(ITensor<TNumber> tensor, ITensor<TNumber> result, long[] strides)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var tensorBlock = (SystemMemoryBlock<TNumber>)tensor.Memory;
        var resultBlock = (SystemMemoryBlock<TNumber>)result.Memory;
        var backend = tensor.Device.GetTensorBackend<ManagedTensorBackend>();

        if (tensor.Shape.IsScalar)
        {
            var scalarValue = tensorBlock[0];

            backend
                .ParallelExecutor
                .For(
                    0,
                    result.Shape.ElementCount,
                    ManagedTensorBackend.MaxDegreeOfParallelism,
                    i => resultBlock[i] = scalarValue);

            return;
        }

        var rank = result.Shape.Rank;
        var resultDims = result.Shape.Dimensions;
        var resultStrides = result.Shape.Strides;
        var srcPtr = tensorBlock.Pointer;
        var dstPtr = resultBlock.Pointer;

        backend
            .ParallelExecutor
            .For(
                0,
                resultDims[0],
                ManagedTensorBackend.MaxDegreeOfParallelism,
                outerIdx =>
                {
                    var baseSrcOffset = outerIdx * strides[0];
                    var baseDstOffset = outerIdx * resultStrides[0];

                    RecursiveBroadcast(
                        depth: 1,
                        rank: rank,
                        srcOffset: baseSrcOffset,
                        dstOffset: baseDstOffset,
                        resultDims: resultDims,
                        resultStrides: resultStrides,
                        broadcastStrides: strides,
                        srcPtr: srcPtr,
                        dstPtr: dstPtr);
                });
    }

    private static unsafe void RecursiveBroadcast<TNumber>(
        int depth,
        int rank,
        long srcOffset,
        long dstOffset,
        int[] resultDims,
        long[] resultStrides,
        long[] broadcastStrides,
        TNumber* srcPtr,
        TNumber* dstPtr)
        where TNumber : unmanaged, INumber<TNumber>
    {
        if (depth == rank - 1)
        {
            var count = resultDims[depth];
            var srcStride = broadcastStrides[depth];
            var dstStride = resultStrides[depth];
            var src = srcPtr + srcOffset;
            var dst = dstPtr + dstOffset;

            if (srcStride == 0)
            {
                var val = *src;
                for (var i = 0; i < count; i++)
                {
                    dst[i * dstStride] = val;
                }
            }
            else if (srcStride == 1 && dstStride == 1)
            {
                Buffer.MemoryCopy(src, dst, count * Unsafe.SizeOf<TNumber>(), count * Unsafe.SizeOf<TNumber>());
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    dst[i * dstStride] = src[i * srcStride];
                }
            }
        }
        else
        {
            var dimSize = resultDims[depth];
            var srcStride = broadcastStrides[depth];
            var dstStride = resultStrides[depth];

            for (var i = 0; i < dimSize; i++)
            {
                RecursiveBroadcast(
                    depth + 1,
                    rank,
                    srcOffset + (i * srcStride),
                    dstOffset + (i * dstStride),
                    resultDims,
                    resultStrides,
                    broadcastStrides,
                    srcPtr,
                    dstPtr);
            }
        }
    }
}