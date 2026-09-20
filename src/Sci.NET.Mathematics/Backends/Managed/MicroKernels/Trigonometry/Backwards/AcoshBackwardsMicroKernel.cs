// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class AcoshBackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
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
        var sqrt = TNumber.Sqrt((left * left) - TNumber.One);
        return right * (TNumber.One / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var sqrt = MathF.Sqrt((left * left) - 1.0f);
        return right * (1.0f / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var sqrt = Math.Sqrt((left * left) - 1.0d);
        return right * (1.0d / sqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var one = Vector256.Create(1.0f);
        var leftSquared = Avx.Multiply(left, left);
        var sqrt = Avx.Sqrt(Avx.Subtract(leftSquared, one));
        var reciprocalSqrt = Avx.Divide(one, sqrt);
        return Avx.Multiply(right, reciprocalSqrt);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var one = Vector256.Create(1.0d);
        var leftSquared = Avx.Multiply(left, left);
        var sqrt = Avx.Sqrt(Avx.Subtract(leftSquared, one));
        var reciprocalSqrt = Avx.Divide(one, sqrt);
        return Avx.Multiply(right, reciprocalSqrt);
    }
}
