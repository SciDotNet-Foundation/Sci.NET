// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class AsecBackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, IRootFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber left, TNumber right)
    {
        var x2 = left * left;
        var sqrt = TNumber.Sqrt(TNumber.One - (TNumber.One / x2));
        var denominator = TNumber.One / (x2 * sqrt);
        return right * denominator;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var x2 = left * left;
        var sqrt = MathF.Sqrt(1.0f - (1.0f / x2));
        var denominator = 1.0f / (x2 * sqrt);
        return right * denominator;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var x2 = left * left;
        var sqrt = Math.Sqrt(1.0d - (1.0d / x2));
        var denominator = 1.0d / (x2 * sqrt);
        return right * denominator;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var x2 = Avx.Multiply(left, left);
        var reciprocalX2 = Avx.Divide(Vector256.Create(1.0f), x2);
        var sqrt = Avx.Sqrt(Avx.Subtract(Vector256.Create(1.0f), reciprocalX2));
        var denominator = Avx.Divide(Vector256.Create(1.0f), Avx.Multiply(x2, sqrt));

        return Avx.Multiply(right, denominator);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var x2 = Avx.Multiply(left, left);
        var reciprocalX2 = Avx.Divide(Vector256.Create(1.0d), x2);
        var sqrt = Avx.Sqrt(Avx.Subtract(Vector256.Create(1.0d), reciprocalX2));
        var denominator = Avx.Divide(Vector256.Create(1.0d), Avx.Multiply(x2, sqrt));

        return Avx.Multiply(right, denominator);
    }
}
