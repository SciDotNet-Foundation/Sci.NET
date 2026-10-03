// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Reduction;

internal class ReduceMaxMicroKernel<TNumber> : IReductionOperation<TNumber>, IReductionOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    public static TNumber Identity => GenericMath.MinValue<TNumber>();

    public static Vector256<float> Avx256Fp32Identity => Vector256.Create(float.MinValue);

    public static Vector256<double> Avx256Fp64Identity => Vector256.Create(double.MinValue);

    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Accumulate(TNumber current, TNumber value)
    {
        return TNumber.Max(current, value);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Finalize(TNumber accumulated, long count)
    {
        return accumulated;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> AccumulateAvxFp32(Vector256<float> current, Vector256<float> values)
    {
        return Avx.Max(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> AccumulateAvxFp64(Vector256<double> current, Vector256<double> values)
    {
        return Avx.Max(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float HorizontalReduceAvxFp32(Vector256<float> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var max128 = Sse.Max(low, high);

        var shuffled = Sse.Shuffle(max128, max128, 0b01_00_11_10);
        max128 = Sse.Max(max128, shuffled);

        shuffled = Sse.Shuffle(max128, max128, 0b00_00_00_01);
        max128 = Sse.Max(max128, shuffled);

        return max128.ToScalar();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double HorizontalReduceAvxFp64(Vector256<double> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var max128 = Sse2.Max(low, high);

        var shuffled = Sse2.Shuffle(max128, max128, 0b01);
        max128 = Sse2.Max(max128, shuffled);

        return max128.ToScalar();
    }
}
