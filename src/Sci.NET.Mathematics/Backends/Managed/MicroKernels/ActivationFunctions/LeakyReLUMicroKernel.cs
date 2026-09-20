// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

// ReSharper disable once InconsistentNaming
internal class LeakyReLUMicroKernel<TNumber> : IUnaryParameterizedOperation<LeakyReLUMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<LeakyReLUMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public LeakyReLUMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, LeakyReLUMicroKernel<TNumber> instance)
    {
        return input > TNumber.Zero ? input : instance._alpha.ScalarValue * input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, LeakyReLUMicroKernel<TNumber> instance)
    {
        return input > 0.0f ? input : instance._alpha.ScalarFp32Value * input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, LeakyReLUMicroKernel<TNumber> instance)
    {
        return input > 0.0d ? input : instance._alpha.ScalarFp64Value * input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, LeakyReLUMicroKernel<TNumber> instance)
    {
        var zero = Vector256<float>.Zero;
        var mask = Avx.Compare(input, zero, FloatComparisonMode.OrderedGreaterThanSignaling);
        var scaled = Avx.Multiply(input, instance._alpha.Vector256ValueFp32);

        return Avx.BlendVariable(scaled, input, mask);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, LeakyReLUMicroKernel<TNumber> instance)
    {
        var zero = Vector256<double>.Zero;
        var mask = Avx.Compare(input, zero, FloatComparisonMode.OrderedGreaterThanSignaling);
        var scaled = Avx.Multiply(input, instance._alpha.Vector256ValueFp64);

        return Avx.BlendVariable(scaled, input, mask);
    }
}
