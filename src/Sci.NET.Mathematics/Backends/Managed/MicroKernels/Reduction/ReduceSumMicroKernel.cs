// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Reduction;

internal class ReduceSumMicroKernel<TNumber> : IReductionOperation<TNumber>, IReductionOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    public static TNumber Identity => TNumber.Zero;

    public static Vector256<float> Avx256Fp32Identity => Vector256<float>.Zero;

    public static Vector256<double> Avx256Fp64Identity => Vector256<double>.Zero;

    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Accumulate(TNumber current, TNumber value)
    {
        return current + value;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Finalize(TNumber accumulated, long count)
    {
        return accumulated;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> AccumulateAvxFp32(Vector256<float> current, Vector256<float> values)
    {
        return Avx.Add(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> AccumulateAvxFp64(Vector256<double> current, Vector256<double> values)
    {
        return Avx.Add(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float HorizontalReduceAvxFp32(Vector256<float> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var sum128 = Sse.Add(low, high);

        sum128 = Sse3.HorizontalAdd(sum128, sum128);
        sum128 = Sse3.HorizontalAdd(sum128, sum128);

        return sum128.ToScalar();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double HorizontalReduceAvxFp64(Vector256<double> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var sum128 = Sse2.Add(low, high);

        sum128 = Sse3.HorizontalAdd(sum128, sum128);

        return sum128.ToScalar();
    }
}
