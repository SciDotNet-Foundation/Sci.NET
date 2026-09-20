// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Arithmetic;

internal class DivideByScalarMicroKernel<TNumber> : IUnaryParameterizedOperation<DivideByScalarMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<DivideByScalarMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _divisor;

    public DivideByScalarMicroKernel(MicroKernelParameter<TNumber> divisor)
    {
        _divisor = divisor;
    }

    public static bool IsAvx2Supported()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, DivideByScalarMicroKernel<TNumber> instance)
    {
        return input / instance._divisor.ScalarValue;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, DivideByScalarMicroKernel<TNumber> instance)
    {
        return input / instance._divisor.ScalarFp32Value;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, DivideByScalarMicroKernel<TNumber> instance)
    {
        return input / instance._divisor.ScalarFp64Value;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, DivideByScalarMicroKernel<TNumber> instance)
    {
        return Avx.Divide(input, instance._divisor.Vector256ValueFp32);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, DivideByScalarMicroKernel<TNumber> instance)
    {
        return Avx.Divide(input, instance._divisor.Vector256ValueFp64);
    }
}
