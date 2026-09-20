// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Arithmetic;

internal class NegateMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return true;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        return -input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        return -input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        return -input;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input)
    {
        return Avx.Xor(input, Vector256.Create(-0.0f));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input)
    {
        return Avx.Xor(input, Vector256.Create(-0.0d));
    }
}
