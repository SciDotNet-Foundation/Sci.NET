// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Fused;

internal class FusedUnaryOperation<TFirstOp, TSecondOp, TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TFirstOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TSecondOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return TFirstOp.HasAvx2Implementation() && TSecondOp.HasAvx2Implementation();
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        var intermediate = TFirstOp.ApplyScalar(input);
        return TSecondOp.ApplyScalar(intermediate);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        var intermediate = TFirstOp.ApplyScalarFp32(input);
        return TSecondOp.ApplyScalarFp32(intermediate);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        var firstResult = TFirstOp.ApplyScalarFp64(input);
        return TSecondOp.ApplyScalarFp64(firstResult);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input)
    {
        var intermediate = TFirstOp.ApplyAvx2Fp32(input);
        return TSecondOp.ApplyAvx2Fp32(intermediate);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input)
    {
        var intermediate = TFirstOp.ApplyAvx2Fp64(input);
        return TSecondOp.ApplyAvx2Fp64(intermediate);
    }
}
