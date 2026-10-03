// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class Asec2BackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, ITrigonometricFunctions<TNumber>, IRootFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber left, TNumber right)
    {
        var two = TNumber.One + TNumber.One;
        var x2 = left * left;
        var sqrt = TNumber.Sqrt(TNumber.One - (TNumber.One / x2));
        var denominator = x2 * sqrt;
        var acsc = TNumber.Acos(TNumber.One / left);
        var derivative = two * acsc / denominator;

        return right * derivative;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var x2 = left * left;
        var sqrt = MathF.Sqrt(1.0f - (1.0f / x2));
        var denominator = x2 * sqrt;
        var acsc = MathF.Acos(1.0f / left);
        var derivative = 2.0f * acsc / denominator;

        return right * derivative;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var x2 = left * left;
        var sqrt = Math.Sqrt(1.0d - (1.0d / x2));
        var denominator = x2 * sqrt;
        var acsc = Math.Acos(1.0d / left);
        var derivative = 2.0d * acsc / denominator;

        return right * derivative;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
