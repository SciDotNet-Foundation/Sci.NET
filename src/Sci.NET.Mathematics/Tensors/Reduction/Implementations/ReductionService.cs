// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Tensors.Common;

namespace Sci.NET.Mathematics.Tensors.Reduction.Implementations;

internal class ReductionService : IReductionService
{
    private readonly IGradientAppenderService _gradientAppenderService;

    public ReductionService()
    {
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
    }

    public ITensor<TNumber> ReduceToShape<TNumber>(ITensor<TNumber> tensor, Shape targetShape)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var currentShape = tensor.Shape;
        var axesToReduce = new List<int>();

        for (var i = 0; i < currentShape.Rank; i++)
        {
            if (currentShape[i] != targetShape[i])
            {
                axesToReduce.Add(i);
            }
        }

        if (axesToReduce.Count > 0)
        {
            return tensor.Sum(axesToReduce.ToArray());
        }

        throw new InvalidShapeException($"The tensor with shape {currentShape} cannot be reduced to the target shape {targetShape}.");
    }

    public bool CanReduceToShape<TNumber>(ITensor<TNumber> tensor, Shape shape)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var originalShape = tensor.Shape.Dimensions;
        var targetShape = shape.Dimensions;

        var originalRank = originalShape.Length;
        var targetRank = targetShape.Length;

        var j = targetRank - 1;
        for (var i = originalRank - 1; i >= 0; i--)
        {
            if (j < 0)
            {
                continue;
            }

            if (originalShape[i] == targetShape[j])
            {
                j--;
            }
            else if (originalShape[i] <= targetShape[j])
            {
                return false;
            }
        }

        return j < 0;
    }

    public ITensor<TNumber> Sum<TNumber>(ITensor<TNumber> tensor, int[]? axes = null, bool keepDims = false)
        where TNumber : unmanaged, INumber<TNumber>
    {
        axes = ValidateAxesForReduction(tensor, axes);
        var resultShape = CalculateResultShape(tensor.Shape.Dimensions, axes, keepDims);

        var result = new Tensor<TNumber>(
            resultShape,
            tensor.Backend,
            requiresGradient: tensor.RequiresGradient);

        tensor.Backend.Reduction.ReduceAdd(tensor, axes, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad => grad.Broadcast(tensor.Shape));

        if (!keepDims)
        {
            return result;
        }

        return result.Broadcast(tensor.Shape);
    }

    public ITensor<TNumber> Mean<TNumber>(
        ITensor<TNumber> tensor,
        int[]? axes = null,
        bool keepDims = false)
        where TNumber : unmanaged, INumber<TNumber>
    {
        axes = ValidateAxesForReduction(tensor, axes);
        var resultShape = CalculateResultShape(tensor.Shape.Dimensions, axes, keepDims);

        var result = new Tensor<TNumber>(
            resultShape,
            tensor.Backend,
            requiresGradient: tensor.RequiresGradient);

        tensor.Backend.Reduction.ReduceMean(tensor, axes, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad =>
            {
                var gradExpanded = grad.Broadcast(tensor.Shape);
                var scale = TNumber.One / TNumber.CreateChecked(tensor.Shape.ElementCount);
                using var scaleTensor = new Scalar<TNumber>(scale, tensor.Backend, requiresGradient: false);
                return gradExpanded.Multiply(scaleTensor);
            });

        if (!keepDims)
        {
            return result;
        }

        return result.Broadcast(tensor.Shape);
    }

    public ITensor<TNumber> Max<TNumber>(
        ITensor<TNumber> tensor,
        int[]? axes = null,
        bool keepDims = false)
        where TNumber : unmanaged, INumber<TNumber>
    {
        axes = ValidateAxesForReduction(tensor, axes);
        var resultShape = CalculateResultShape(tensor.Shape.Dimensions, axes, keepDims);

        var result = new Tensor<TNumber>(
            resultShape,
            tensor.Backend,
            requiresGradient: tensor.RequiresGradient);

        tensor.Backend.Reduction.ReduceMax(tensor, axes, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad =>
            {
                var gradExpanded = grad.Broadcast(tensor.Shape);
                using var broadcastedMax = result.Broadcast(tensor.Shape);
                using var mask = tensor.PointwiseEquals(broadcastedMax);

                return gradExpanded.Multiply(mask);
            });

        if (!keepDims)
        {
            return result;
        }

        return result.Broadcast(tensor.Shape);
    }

    public ITensor<TNumber> Min<TNumber>(
        ITensor<TNumber> tensor,
        int[]? axes = null,
        bool keepDims = false)
        where TNumber : unmanaged, INumber<TNumber>
    {
        axes = ValidateAxesForReduction(tensor, axes);
        var resultShape = CalculateResultShape(tensor.Shape.Dimensions, axes, keepDims);

        var result = new Tensor<TNumber>(
            resultShape,
            tensor.Backend,
            requiresGradient: tensor.RequiresGradient);

        tensor.Backend.Reduction.ReduceMin(tensor, axes, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad =>
            {
                var gradExpanded = grad.Broadcast(tensor.Shape);

                using var broadcastedMin = result.Broadcast(tensor.Shape);
                using var mask = tensor.PointwiseEquals(broadcastedMin);

                return gradExpanded.Multiply(mask);
            });

        if (!keepDims)
        {
            return result;
        }

        return result.Broadcast(tensor.Shape);
    }

    private static int[] ValidateAxesForReduction<TNumber>(ITensor<TNumber> tensor, int[]? axes)
        where TNumber : unmanaged, INumber<TNumber>
    {
        if (axes is not null && axes.Length > tensor.Shape.Rank)
        {
            throw new InvalidShapeException($"The number of axes to sum over cannot exceed the number of dimensions in shape {tensor.Shape}.");
        }

        axes ??= Enumerable.Range(0, tensor.Shape.Rank).ToArray();

        if (axes.Any(x => x < 0 || x >= tensor.Shape.Rank))
        {
            throw new InvalidShapeException($"The axes to sum over must be within the bounds of the tensor with shape {tensor.Shape}.");
        }

        return axes;
    }

    private static Shape CalculateResultShape(int[] shape, int[]? axes, bool keepDims)
    {
        var axisSet = axes is not null ? [.. axes] : new HashSet<int>();

        var resultShapeDimensions = new int[shape.Length];

        for (var i = 0; i < shape.Length; i++)
        {
#pragma warning disable IDE0045
            if (axisSet.Contains(i))
#pragma warning restore IDE0045
            {
                resultShapeDimensions[i] = keepDims ? 1 : 0;
            }
            else
            {
                resultShapeDimensions[i] = shape[i];
            }
        }

        return new Shape(resultShapeDimensions.Where(dim => dim != 0).ToArray());
    }
}