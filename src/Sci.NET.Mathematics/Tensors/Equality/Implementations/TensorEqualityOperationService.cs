// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Tensors.Common;

namespace Sci.NET.Mathematics.Tensors.Equality.Implementations;

internal class TensorEqualityOperationService : ITensorEqualityOperationService
{
    private readonly IDeviceGuardService _guardService;
    private readonly IGradientAppenderService _gradientAppenderService;

    public TensorEqualityOperationService()
    {
        _guardService = TensorServiceProvider.GetTensorOperationServiceProvider().GetDeviceGuardService();
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
    }

    public ITensor<TNumber> PointwiseEquals<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseEqual(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> PointwiseNotEqual<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseNotEqual(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> PointwiseGreaterThan<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseGreaterThan(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> PointwiseGreaterThanOrEqual<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseGreaterThanOrEqual(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> PointwiseLessThan<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseLessThan(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> PointwiseLessThanOrEqual<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentElementCount(left.Shape, right.Shape);
        var backend = _guardService.GuardBinaryOperation(left.Device, right.Device);

        var result = new Tensor<TNumber>(left.Shape, backend);

        backend.EqualityOperations.PointwiseLessThanOrEqual(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }
}