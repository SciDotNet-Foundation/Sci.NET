// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors.Common;
using Sci.NET.Mathematics.Tensors.Manipulation;

namespace Sci.NET.Mathematics.Tensors.Pointwise.Implementations;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The concrete type is not known at compile time.")]
internal class ArithmeticService : IArithmeticService
{
    private readonly IDeviceGuardService _deviceGuardService;
    private readonly IGradientAppenderService _gradientAppenderService;
    private readonly IBroadcastService _broadcastService;

    public ArithmeticService()
    {
        _deviceGuardService = TensorServiceProvider.GetTensorOperationServiceProvider().GetDeviceGuardService();
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
        _broadcastService = TensorServiceProvider.GetTensorOperationServiceProvider().GetBroadcastingService();
    }

    public ITensor<TNumber> Add<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var device = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var outputShape = GetBinaryOpOutputShape(left.Shape, right.Shape);
        var result = new Tensor<TNumber>(outputShape, device, left.RequiresGradient || right.RequiresGradient);

        device.Arithmetic.Add(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad);

        return result;
    }

    public ITensor<TNumber> Subtract<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var device = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var outputShape = GetBinaryOpOutputShape(left.Shape, right.Shape);
        var result = new Tensor<TNumber>(outputShape, device, left.RequiresGradient || right.RequiresGradient);

        device.Arithmetic.Subtract(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad => grad,
            grad => grad.Negate());

        return result;
    }

    public ITensor<TNumber> Multiply<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var device = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var outputShape = GetBinaryOpOutputShape(left.Shape, right.Shape);
        var result = new Tensor<TNumber>(outputShape, device, left.RequiresGradient || right.RequiresGradient);

        device.Arithmetic.Multiply(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            right.Multiply,
            left.Multiply);

        return result;
    }

    public void MultiplyInplace<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var device = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var outputShape = GetBinaryOpOutputShape(left.Shape, right.Shape);
        var result = new Tensor<TNumber>(outputShape, device, left.RequiresGradient || right.RequiresGradient);

        device.Arithmetic.Divide(left, right, left);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            right.Divide,
            grad => grad.Multiply(left).Divide(right.Square()).Negate());
    }

    public ITensor<TNumber> Divide<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var device = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var outputShape = GetBinaryOpOutputShape(left.Shape, right.Shape);
        var result = new Tensor<TNumber>(outputShape, device, left.RequiresGradient || right.RequiresGradient);

        device.Arithmetic.Divide(left, right, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            right.Divide,
            grad => grad.Multiply(left).Divide(right.Square()).Negate());

        return result;
    }

    public ITensor<TNumber> Negate<TNumber>(ITensor<TNumber> value)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var backend = value.Backend;

        if (!GenericMath.IsSigned<TNumber>())
        {
            var newMemoryBlock = value.Memory.Copy();
            var resultShortcut = new Tensor<TNumber>(newMemoryBlock, value.Shape, backend, value.RequiresGradient);

            _gradientAppenderService.AddGradientIfRequired(
                ref resultShortcut,
                value,
                null,
                grad => grad.Negate());

            return resultShortcut;
        }

        var result = new Tensor<TNumber>(backend, value.RequiresGradient, value.Shape.Dimensions);

        backend.Arithmetic.Negate(value, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            value,
            null,
            grad => grad.Negate());

        return result;
    }

    public ITensor<TNumber> Abs<TNumber>(ITensor<TNumber> value)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var backend = value.Backend;

        if (!GenericMath.IsSigned<TNumber>())
        {
            var newMemoryBlock = value.Memory.Copy();

            var resultShortcut = new Tensor<TNumber>(newMemoryBlock, value.Shape, backend, value.RequiresGradient);

            _gradientAppenderService.AddGradientIfRequired(
                ref resultShortcut,
                value,
                null,
                grad => grad);

            return resultShortcut;
        }

        var result = new Tensor<TNumber>(backend, value.RequiresGradient, value.Shape.Dimensions);

        backend.Arithmetic.Abs(value, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            value,
            null,
            grad =>
            {
                var gradResult = new Tensor<TNumber>(value.Shape, value.Backend);
                value.Backend.Arithmetic.AbsGradient(value, grad, gradResult);

                return gradResult;
            });

        return result;
    }

    public ITensor<TNumber> Sqrt<TNumber>(ITensor<TNumber> tensor)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var backend = tensor.Backend;
        var result = new Tensor<TNumber>(backend, tensor.RequiresGradient, tensor.Shape.Dimensions);

        backend.Arithmetic.Sqrt(tensor, result);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            tensor,
            null,
            grad =>
            {
                using var two = new Scalar<TNumber>(TNumber.CreateChecked(2), backend);
                using var one = new Scalar<TNumber>(TNumber.CreateChecked(1), backend);
                using var localGradient = one.Divide(result.Multiply(two));

                return grad.Multiply(localGradient);
            });

        return result;
    }

    private Shape GetBinaryOpOutputShape(Shape left, Shape right)
    {
        Shape bigger, smaller;

        if (left.Rank < right.Rank)
        {
            bigger = right;
            smaller = left;
        }
        else
        {
            bigger = left;
            smaller = right;
        }

        if (!_broadcastService.CanBroadcastTo(smaller, bigger))
        {
            throw new InvalidShapeException($"Cannot broadcast shapes {left} and {right} for a binary operation.");
        }

        return bigger;
    }
}