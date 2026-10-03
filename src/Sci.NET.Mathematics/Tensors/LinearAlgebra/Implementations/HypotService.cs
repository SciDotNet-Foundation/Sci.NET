// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Tensors.Common;
using Sci.NET.Mathematics.Tensors.Manipulation;

namespace Sci.NET.Mathematics.Tensors.LinearAlgebra.Implementations;

internal class HypotService : IHypotService
{
    private readonly IDeviceGuardService _deviceGuardService;
    private readonly IBroadcastService _broadcastService;
    private readonly IGradientAppenderService _gradientAppenderService;

    public HypotService()
    {
        _deviceGuardService = TensorServiceProvider.GetTensorOperationServiceProvider().GetDeviceGuardService();
        _broadcastService = TensorServiceProvider.GetTensorOperationServiceProvider().GetBroadcastingService();
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
    }

    public ITensor<TNumber> Hypot<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        var backend = _deviceGuardService.GuardBinaryOperation(left.Device, right.Device);
        var (leftBroadcasted, rightBroadcasted) = _broadcastService.Broadcast(left, right);

        var result = new Tensor<TNumber>(leftBroadcasted.Shape, backend, left.RequiresGradient || right.RequiresGradient);

        _gradientAppenderService.AddGradientIfRequired(
            ref result,
            left,
            right,
            null,
            grad =>
            {
                using var quotient = left.Divide(result);
                return grad.Multiply(quotient);
            },
            grad =>
            {
                using var quotient = right.Divide(result);
                return grad.Multiply(quotient);
            });

        backend.LinearAlgebra.Hypot(
            leftBroadcasted,
            rightBroadcasted,
            result);

        return result;
    }
}
