// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Trigonometry.Backwards;

internal class Atan2BackwardsMicroKernel<TNumber> : IBinaryOperation<TNumber>, IBinaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, IRootFunctions<TNumber>, ITrigonometricFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber left, TNumber right)
    {
        var two = TNumber.CreateChecked(2);
        var atan = TNumber.Atan(left);
        var twoAtan = two * atan;
        var x2Plus1 = (left * left) + TNumber.One;

        return right * (twoAtan / x2Plus1);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float left, float right)
    {
        var atan = MathF.Atan(left);
        var twoAtan = 2.0f * atan;
        var x2Plus1 = (left * left) + 1.0f;

        return right * (twoAtan / x2Plus1);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double left, double right)
    {
        var atan = Math.Atan(left);
        var twoAtan = 2.0d * atan;
        var x2Plus1 = (left * left) + 1.0d;

        return right * (twoAtan / x2Plus1);
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
