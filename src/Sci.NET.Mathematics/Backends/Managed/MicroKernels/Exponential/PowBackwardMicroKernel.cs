// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Exponential;

internal class PowBackwardMicroKernel<TNumber> : IUnaryParameterizedOperation<PowBackwardMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<PowBackwardMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IPowerFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _exponentParameter;

    public PowBackwardMicroKernel(MicroKernelParameter<TNumber> exponentParameter)
    {
        _exponentParameter = exponentParameter;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, PowBackwardMicroKernel<TNumber> instance)
    {
        return input * TNumber.Pow(input, instance._exponentParameter.ScalarValue - TNumber.One);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, PowBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, PowBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, PowBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, PowBackwardMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
