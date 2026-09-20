// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class CeluMicroKernel<TNumber> : IUnaryParameterizedOperation<CeluMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<CeluMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public CeluMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, CeluMicroKernel<TNumber> instance)
    {
        return TNumber.Max(TNumber.Zero, input) + TNumber.Min(TNumber.Zero, instance._alpha.ScalarValue * (TNumber.Exp(input / instance._alpha.ScalarValue) - TNumber.One));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, CeluMicroKernel<TNumber> instance)
    {
        return MathF.Max(0f, input) + MathF.Min(0f, instance._alpha.ScalarFp32Value * (MathF.Exp(input / instance._alpha.ScalarFp32Value) - 1f));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, CeluMicroKernel<TNumber> instance)
    {
        return Math.Max(0.0, input) + Math.Min(0.0, instance._alpha.ScalarFp64Value * (Math.Exp(input / instance._alpha.ScalarFp64Value) - 1.0));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, CeluMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, CeluMicroKernel<TNumber> instance)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
