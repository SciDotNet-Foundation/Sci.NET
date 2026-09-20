// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class SwishBackwardMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
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
        var expNegX = TNumber.Exp(-input);
        return ((expNegX * (input + TNumber.One)) + TNumber.One) / ((TNumber.One + expNegX) * (TNumber.One + expNegX));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        var expNegX = MathF.Exp(-input);
        return ((expNegX * (input + 1.0f)) + 1.0f) / ((1.0f + expNegX) * (1.0f + expNegX));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        var expNegX = Math.Exp(-input);
        return ((expNegX * (input + 1.0)) + 1.0) / ((1.0 + expNegX) * (1.0 + expNegX));
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
