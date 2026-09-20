// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Exponential;

internal class ExpMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        return TNumber.Exp(input);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        return MathF.Exp(input);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        return Math.Exp(input);
    }

    [ExcludeFromCodeCoverage]
    public static Vector256<float> ApplyAvx2Fp32(Vector256<float> input)
    {
        throw new IntrinsicTypeNotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    public static Vector256<double> ApplyAvx2Fp64(Vector256<double> input)
    {
        throw new IntrinsicTypeNotImplementedException();
    }
}
