// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Tensors.Common;

namespace Sci.NET.Mathematics.Tensors.Manipulation.Implementations;

internal class BroadcastService : IBroadcastService
{
    private readonly IGradientAppenderService _gradientAppenderService;

    public BroadcastService()
    {
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
    }

    public bool CanBroadcastTo(Shape source, Shape target)
    {
        if (source.Rank > target.Rank)
        {
            return false;
        }

        var padDims = target.Rank - source.Rank;
        var padShape = Enumerable.Repeat(1, padDims).Concat(source.Dimensions).ToArray();

        foreach (var (targetDim, sourceDim) in target.Dimensions.Reverse().Zip(padShape.AsEnumerable().Reverse()))
        {
            if (sourceDim != 1 && sourceDim != targetDim)
            {
                return false;
            }
        }

        return true;
    }

    public ITensor<TNumber> Broadcast<TNumber>(ITensor<TNumber> tensor, Shape targetShape)
        where TNumber : unmanaged, INumber<TNumber>
    {
        if (tensor.Shape == targetShape)
        {
            return tensor;
        }

        if (!CanBroadcastTo(tensor.Shape, targetShape))
        {
            throw new InvalidShapeException($"Cannot broadcast shapes {tensor.Shape} and {targetShape}.");
        }

        var padDims = targetShape.Rank - tensor.Shape.Rank;
        var padShape = Enumerable.Repeat(1, padDims).Concat(tensor.Shape.Dimensions).ToArray();
        var broadcastStrides = Enumerable.Repeat(1L, padShape.Length).ToArray();

        var result = new Tensor<TNumber>(targetShape, tensor.Backend, requiresGradient: tensor.RequiresGradient);

        for (var i = 0; i < padShape.Length; i++)
        {
            if (padShape[i] == 1 && targetShape[i] > 1)
            {
                broadcastStrides[i] = 0;
            }
            else
            {
                var sourceIdx = i - padDims;
                broadcastStrides[i] = sourceIdx >= 0 ? tensor.Shape.Strides[sourceIdx] : 0;
            }
        }

        // TODO: We shouldn't create a new tensor here, but the old kernels dont support iterating by strides.
        tensor.Backend.Broadcasting.Broadcast(tensor, result, broadcastStrides);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad => grad.Sum([.. Enumerable.Range(tensor.Shape.Rank, targetShape.Rank - tensor.Shape.Rank)]));

        return result.Reshape(targetShape);
    }

    public (ITensor<TNumber> Left, ITensor<TNumber> Right) Broadcast<TNumber>(
        ITensor<TNumber> left,
        ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var shouldSwap = left.Shape.Rank < right.Shape.Rank;

        var (bigger, smaller) = shouldSwap ? (right, left) : (left, right);

        if (!CanBroadcastTo(smaller.Shape, bigger.Shape))
        {
            throw new InvalidShapeException($"Cannot broadcast shapes {left.Shape} and {right.Shape}.");
        }

        var broadcast = Broadcast(smaller, bigger.Shape);

        return shouldSwap ? (broadcast, bigger) : (bigger, broadcast);
    }
}