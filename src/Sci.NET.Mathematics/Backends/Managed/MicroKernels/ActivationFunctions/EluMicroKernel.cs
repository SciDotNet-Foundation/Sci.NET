// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class EluMicroKernel<TNumber> : IUnaryParameterizedOperation<EluMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<EluMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public EluMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, EluMicroKernel<TNumber> instance)
    {
        return input > TNumber.Zero ? input : instance._alpha.ScalarValue * (TNumber.Exp(input) - TNumber.One);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, EluMicroKernel<TNumber> instance)
    {
        return input > 0f ? input : instance._alpha.ScalarFp32Value * (MathF.Exp(input) - 1f);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, EluMicroKernel<TNumber> instance)
    {
        return input > 0.0 ? input : instance._alpha.ScalarFp64Value * (Math.Exp(input) - 1.0);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, EluMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, EluMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
