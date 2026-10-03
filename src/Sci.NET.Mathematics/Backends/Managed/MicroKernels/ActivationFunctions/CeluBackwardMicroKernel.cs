// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class CeluBackwardMicroKernel<TNumber> : IUnaryParameterizedOperation<CeluBackwardMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<CeluBackwardMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public CeluBackwardMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, CeluBackwardMicroKernel<TNumber> instance)
    {
        return TNumber.Min(TNumber.One, instance._alpha.ScalarValue * TNumber.Exp(input / instance._alpha.ScalarValue));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, CeluBackwardMicroKernel<TNumber> instance)
    {
        return MathF.Min(1f, instance._alpha.ScalarFp32Value * MathF.Exp(input / instance._alpha.ScalarFp32Value));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, CeluBackwardMicroKernel<TNumber> instance)
    {
        return Math.Min(1.0, instance._alpha.ScalarFp64Value * Math.Exp(input / instance._alpha.ScalarFp64Value));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, CeluBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, CeluBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
