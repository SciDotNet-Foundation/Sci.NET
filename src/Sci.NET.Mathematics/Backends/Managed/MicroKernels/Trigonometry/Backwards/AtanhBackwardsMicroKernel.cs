// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class AtanhBackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
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
        var oneMinusX2 = TNumber.One - (left * left);
        return right * (TNumber.One / oneMinusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var oneMinusX2 = 1.0f - (left * left);
        return right * (1.0f / oneMinusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var oneMinusX2 = 1.0d - (left * left);
        return right * (1.0d / oneMinusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var one = Vector256.Create(1.0f);
        var leftSquared = Avx.Multiply(left, left);
        var oneMinusX2 = Avx.Subtract(one, leftSquared);
        var reciprocalOneMinusX2 = Avx.Divide(one, oneMinusX2);
        return Avx.Multiply(right, reciprocalOneMinusX2);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var one = Vector256.Create(1.0d);
        var leftSquared = Avx.Multiply(left, left);
        var oneMinusX2 = Avx.Subtract(one, leftSquared);
        var reciprocalOneMinusX2 = Avx.Divide(one, oneMinusX2);
        return Avx.Multiply(right, reciprocalOneMinusX2);
    }
}
