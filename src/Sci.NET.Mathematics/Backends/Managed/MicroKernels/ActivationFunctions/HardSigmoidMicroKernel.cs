// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class HardSigmoidMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        var three = TNumber.One + TNumber.One + TNumber.One;
        var minusThree = TNumber.Zero - three;
        var six = three + three;
        var two = TNumber.One + TNumber.One;
        var half = TNumber.One / two;

        if (input <= minusThree)
        {
            return TNumber.Zero;
        }
        else if (input >= three)
        {
            return TNumber.One;
        }
        else
        {
            return (input / six) + half;
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        if (input <= -3.0f)
        {
            return 0.0f;
        }
        else if (input >= 3.0f)
        {
            return 1.0f;
        }
        else
        {
            return (input / 6.0f) + 0.5f;
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        if (input <= -3.0f)
        {
            return 0.0f;
        }
        else if (input >= 3.0f)
        {
            return 1.0f;
        }
        else
        {
            return (input / 6.0f) + 0.5f;
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input)
    {
        var negThree = Vector256.Create(-3.0f);
        var posThree = Vector256.Create(3.0f);
        var sixth = Vector256.Create(1.0f / 6.0f);
        var half = Vector256.Create(0.5f);
        var zero = Vector256<float>.Zero;
        var one = Vector256.Create(1.0f);
        var linear = Fma.MultiplyAdd(input, sixth, half);
        var maskLow = Avx.Compare(input, negThree, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling);
        var maskHigh = Avx.Compare(input, posThree, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling);
        var result = Avx.BlendVariable(linear, zero, maskLow);

        return Avx.BlendVariable(result, one, maskHigh);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input)
    {
        var negThree = Vector256.Create(-3.0d);
        var posThree = Vector256.Create(3.0d);
        var sixth = Vector256.Create(1.0d / 6.0d);
        var half = Vector256.Create(0.5d);
        var zero = Vector256<double>.Zero;
        var one = Vector256.Create(1.0d);
        var linear = Fma.MultiplyAdd(input, sixth, half);
        var maskLow = Avx.Compare(input, negThree, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling);
        var maskHigh = Avx.Compare(input, posThree, FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling);
        var result = Avx.BlendVariable(linear, zero, maskLow);

        return Avx.BlendVariable(result, one, maskHigh);
    }
}
