// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class AtanBackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber left, TNumber right)
    {
        var onePlusX2 = (left * left) + TNumber.One;
        return right * (TNumber.One / onePlusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var onePlusX2 = (left * left) + 1.0f;
        return right * (1.0f / onePlusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var onePlusX2 = (left * left) + 1.0d;
        return right * (1.0d / onePlusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var one = Vector256.Create(1.0f);
        var squared = Avx.Multiply(left, left);
        var onePlusX2 = Avx.Add(squared, one);
        var reciprocal = Avx.Divide(one, onePlusX2);
        return Avx.Multiply(right, reciprocal);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var one = Vector256.Create(1.0d);
        var squared = Avx.Multiply(left, left);
        var onePlusX2 = Avx.Add(squared, one);
        var reciprocal = Avx.Divide(one, onePlusX2);
        return Avx.Multiply(right, reciprocal);
    }
}
