// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Casting;

[SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter", Justification = "By design")]
internal class CastTruncatingMicroKernel<TIn, TOut> : IMixedPrecisionUnaryOperation<TIn, TOut>, IMixedPrecisionUnaryOperationAvx2
    where TIn : unmanaged, INumber<TIn>
    where TOut : unmanaged, INumber<TOut>
{
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    /// <inheritdoc />
    public static TOut ApplyScalar(TIn input)
    {
        return TOut.CreateTruncating(input);
    }

    public static double ApplyScalarFp32Fp64(float input)
    {
        return double.CreateTruncating(input);
    }

    public static float ApplyScalarFp64Fp32(double input)
    {
        return float.CreateTruncating(input);
    }

    public static Vector256<double> ApplyAvxFp32Fp64(Vector128<float> input)
    {
        return Avx.ConvertToVector256Double(input);
    }

    public static Vector128<float> ApplyAvxFp64Fp32(Vector256<double> input)
    {
        return Avx.ConvertToVector128Single(input);
    }
}