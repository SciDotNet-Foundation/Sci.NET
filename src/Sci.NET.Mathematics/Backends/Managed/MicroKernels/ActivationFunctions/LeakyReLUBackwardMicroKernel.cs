// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

// ReSharper disable once InconsistentNaming
internal class LeakyReLUBackwardMicroKernel<TNumber> : IUnaryParameterizedOperation<LeakyReLUBackwardMicroKernel<TNumber>, TNumber>,
    IUnaryParameterizedOperationAvx2<LeakyReLUBackwardMicroKernel<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>
{
    private readonly MicroKernelParameter<TNumber> _alpha;

    public LeakyReLUBackwardMicroKernel(MicroKernelParameter<TNumber> alpha)
    {
        _alpha = alpha;
    }

    public static bool IsAvx2Supported()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input, LeakyReLUBackwardMicroKernel<TNumber> instance)
    {
        return input > TNumber.Zero ? TNumber.One : instance._alpha.ScalarValue;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyTailFp32(float input, LeakyReLUBackwardMicroKernel<TNumber> instance)
    {
        return input > 0.0f ? 1.0f : instance._alpha.ScalarFp32Value;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyTailFp64(double input, LeakyReLUBackwardMicroKernel<TNumber> instance)
    {
        return input > 0.0d ? 1.0d : instance._alpha.ScalarFp64Value;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input, LeakyReLUBackwardMicroKernel<TNumber> instance)
    {
        var zero = Vector256<float>.Zero;
        var one = Vector256<float>.One;
        var mask = Avx.Compare(input, zero, FloatComparisonMode.OrderedGreaterThanSignaling);

        return Avx.BlendVariable(instance._alpha.Vector256ValueFp32, one, mask);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input, LeakyReLUBackwardMicroKernel<TNumber> instance)
    {
        var zero = Vector256<double>.Zero;
        var one = Vector256<double>.One;
        var mask = Avx.Compare(input, zero, FloatComparisonMode.OrderedGreaterThanSignaling);

        return Avx.BlendVariable(instance._alpha.Vector256ValueFp64, one, mask);
    }
}
