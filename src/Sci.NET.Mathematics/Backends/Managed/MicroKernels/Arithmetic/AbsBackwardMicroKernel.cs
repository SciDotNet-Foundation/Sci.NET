// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Arithmetic;

internal class AbsBackwardMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
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
        if (left > TNumber.Zero)
        {
            return right;
        }

        if (left < TNumber.Zero)
        {
            return -right;
        }

        return TNumber.Zero;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        if (left > 0.0f)
        {
            return right;
        }

        if (left < 0.0f)
        {
            return -right;
        }

        return 0.0f;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        if (left > 0.0d)
        {
            return right;
        }

        if (left < 0.0d)
        {
            return -right;
        }

        return 0.0d;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvxFp32(Vector256<float> left, Vector256<float> right)
    {
        var minusOne = Vector256.Create(-1.0f);
        var zeroF = Vector256<float>.Zero;

        var positiveMask = Avx.CompareGreaterThan(left, zeroF);
        var negativeMask = Avx.CompareLessThan(left, zeroF);
        var positivePart = Avx.And(positiveMask, right);
        var negativePart = Avx.Multiply(right, minusOne);
        negativePart = Avx.And(negativeMask, negativePart);

        return Avx.Or(positivePart, negativePart);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvxFp64(Vector256<double> left, Vector256<double> right)
    {
        var minusOne = Vector256.Create(-1.0d);
        var zeroF = Vector256<double>.Zero;

        var positiveMask = Avx.CompareGreaterThan(left, zeroF);
        var negativeMask = Avx.CompareLessThan(left, zeroF);
        var positivePart = Avx.And(positiveMask, right);
        var negativePart = Avx.Multiply(right, minusOne);
        negativePart = Avx.And(negativeMask, negativePart);

        return Avx.Or(positivePart, negativePart);
    }
}
