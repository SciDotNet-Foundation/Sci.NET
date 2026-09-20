// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Exponential;

internal class PowMicroKernel<TNumber> : IUnaryParameterizedOperation<PowMicroKernel<TNumber>, TNumber>, IUnaryParameterizedOperationAvx2<PowMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IPowerFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _exponent;

    public PowMicroKernel(MicroKernelParameter<TNumber> exponent)
    {
        _exponent = exponent;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, PowMicroKernel<TNumber> instance)
    {
        return TNumber.Pow(input, instance._exponent.ScalarValue);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, PowMicroKernel<TNumber> instance)
    {
        return MathF.Pow(input, instance._exponent.ScalarFp32Value);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, PowMicroKernel<TNumber> instance)
    {
        return Math.Pow(input, instance._exponent.ScalarFp64Value);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, PowMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, PowMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
