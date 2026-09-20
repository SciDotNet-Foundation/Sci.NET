// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Reduction;

internal class ReduceMinMicroKernel<TNumber> : IReductionOperation<TNumber>, IReductionOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    public static TNumber Identity => GenericMath.MaxValue<TNumber>();

    public static Vector256<float> Avx256Fp32Identity => Vector256.Create(float.MaxValue);

    public static Vector256<double> Avx256Fp64Identity => Vector256.Create(double.MaxValue);

    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Accumulate(TNumber current, TNumber value)
    {
        return TNumber.Min(current, value);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber Finalize(TNumber accumulated, long count)
    {
        return accumulated;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> AccumulateAvxFp32(Vector256<float> current, Vector256<float> values)
    {
        return Avx.Min(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> AccumulateAvxFp64(Vector256<double> current, Vector256<double> values)
    {
        return Avx.Min(current, values);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float HorizontalReduceAvxFp32(Vector256<float> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var min128 = Sse.Min(low, high);
        var shuffled = Sse.Shuffle(min128, min128, 0b01_00_11_10);

        min128 = Sse.Min(min128, shuffled);
        shuffled = Sse.Shuffle(min128, min128, 0b00_00_00_01);
        min128 = Sse.Min(min128, shuffled);

        return min128.ToScalar();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double HorizontalReduceAvxFp64(Vector256<double> vector)
    {
        var low = vector.GetLower();
        var high = vector.GetUpper();
        var min128 = Sse2.Min(low, high);
        var shuffled = Sse2.Shuffle(min128, min128, 0b01);

        min128 = Sse2.Min(min128, shuffled);

        return min128.ToScalar();
    }
}
