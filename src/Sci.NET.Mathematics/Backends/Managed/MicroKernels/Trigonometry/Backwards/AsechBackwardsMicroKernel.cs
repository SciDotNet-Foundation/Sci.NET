// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class AsechBackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
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
        var sqrt = TNumber.Sqrt((TNumber.One / x2) - TNumber.One) * x2;

        return right * (-TNumber.One / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var x2 = left * left;
        var sqrt = MathF.Sqrt((1.0f / x2) - 1.0f) * x2;

        return right * (-1.0f / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var x2 = left * left;
        var sqrt = Math.Sqrt((1.0d / x2) - 1.0d) * x2;

        return right * (-1.0d / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var x2 = Avx.Multiply(left, left);
        var reciprocalX2 = Avx.Divide(Vector256.Create(1.0f), x2);
        var sqrt = Avx.Sqrt(Avx.Subtract(reciprocalX2, Vector256.Create(1.0f)));
        var denominator = Avx.Multiply(sqrt, x2);
        var reciprocalDenominator = Avx.Divide(Vector256.Create(-1.0f), denominator);

        return Avx.Multiply(right, reciprocalDenominator);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var x2 = Avx.Multiply(left, left);
        var reciprocalX2 = Avx.Divide(Vector256.Create(1.0d), x2);
        var sqrt = Avx.Sqrt(Avx.Subtract(reciprocalX2, Vector256.Create(1.0d)));
        var denominator = Avx.Multiply(sqrt, x2);
        var reciprocalDenominator = Avx.Divide(Vector256.Create(-1.0d), denominator);

        return Avx.Multiply(right, reciprocalDenominator);
    }
}
