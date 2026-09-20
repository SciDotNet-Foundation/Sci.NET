// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class EluBackwardMicroKernel<TNumber> : IUnaryParameterizedOperation<EluBackwardMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<EluBackwardMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public EluBackwardMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, EluBackwardMicroKernel<TNumber> instance)
    {
        return input > TNumber.Zero ? TNumber.One : instance._alpha.ScalarValue * TNumber.Exp(input);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, EluBackwardMicroKernel<TNumber> instance)
    {
        return input > 0f ? 1f : instance._alpha.ScalarFp32Value * MathF.Exp(input);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, EluBackwardMicroKernel<TNumber> instance)
    {
        return input > 0.0 ? 1.0 : instance._alpha.ScalarFp64Value * Math.Exp(input);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, EluBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, EluBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
