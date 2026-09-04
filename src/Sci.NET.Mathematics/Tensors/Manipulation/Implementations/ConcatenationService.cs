// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Tensors.Common;

namespace Sci.NET.Mathematics.Tensors.Manipulation.Implementations;

internal class ConcatenationService : IConcatenationService
{
    private readonly IDeviceGuardService _deviceGuardService;
    private readonly IGradientAppenderService _gradientAppenderService;

    public ConcatenationService()
    {
        _deviceGuardService = TensorServiceProvider.GetTensorOperationServiceProvider().GetDeviceGuardService();
        _gradientAppenderService = TensorServiceProvider.GetTensorOperationServiceProvider().GetGradientAppenderService();
    }

    public ITensor<TNumber> Concatenate<TTensor, TNumber>(ICollection<TTensor> tensors)
        where TTensor : ITensor<TNumber>
        where TNumber : unmanaged, INumber<TNumber>
    {
        InvalidShapeException.ThrowIfDifferentShape([.. tensors.Select(t => t.Shape)]);
        var backend = _deviceGuardService.GuardMultiParameterOperation([.. tensors.Select(x => x.Device)]);

        var shape = tensors.First().Shape;
        var newShapeDims = new int[shape.Rank + 1];
        newShapeDims[0] = tensors.Count;

        for (var i = 0; i < shape.Rank; i++)
        {
            newShapeDims[i + 1] = shape[i];
        }

        var newShape = new Shape(newShapeDims);
        var result = new Tensor<TNumber>(newShape, backend);

        for (var i = 0; i < tensors.Count; i++)
        {
            result.Memory.BlockCopyFrom(tensors.ElementAt(i).Memory, 0, i * shape.ElementCount, shape.ElementCount);
        }

        foreach (var tensor in tensors)
        {
            _gradientAppenderService.AddGradientIfRequired(
                ref result,
                tensor,
                null,
                _ => throw new AutoDiffNotSupportedException(nameof(Concatenate)));
        }

        return result;
    }
}