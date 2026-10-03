// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

internal class MishMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, IExponentialFunctions<TNumber>, ILogarithmicFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        // mish\left(x\right)=x\tanh\left(\ln\left(1+e^{x}\right)\right)
        // mish\left(x\right)=x\cdot\frac{e^{\ln\left(e^{x}+1\right)}-e^{-\ln\left(e^{x}+1\right)}}{e^{\ln\left(e^{x}+1\right)}+e^{-\ln\left(e^{x}+1\right)}}
        var expXPlus1 = TNumber.Exp(input) + TNumber.One;
        var lnExpXPlus1 = TNumber.Log(expXPlus1);
        var expLnExpPlus1 = TNumber.Exp(lnExpXPlus1);
        var negExpLnExpPlus1 = TNumber.Exp(-lnExpXPlus1);

        return input * (expLnExpPlus1 - negExpLnExpPlus1) / (expLnExpPlus1 + negExpLnExpPlus1);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        // mish\left(x\right)=x\tanh\left(\ln\left(1+e^{x}\right)\right)
        // mish\left(x\right)=x\cdot\frac{e^{\ln\left(e^{x}+1\right)}-e^{-\ln\left(e^{x}+1\right)}}{e^{\ln\left(e^{x}+1\right)}+e^{-\ln\left(e^{x}+1\right)}}
        var expXPlus1 = MathF.Exp(input) + 1.0f;
        var lnExpXPlus1 = MathF.Log(expXPlus1);
        var expLnExpPlus1 = MathF.Exp(lnExpXPlus1);
        var negExpLnExpPlus1 = MathF.Exp(-lnExpXPlus1);

        return input * (expLnExpPlus1 - negExpLnExpPlus1) / (expLnExpPlus1 + negExpLnExpPlus1);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        // mish\left(x\right)=x\tanh\left(\ln\left(1+e^{x}\right)\right)
        // mish\left(x\right)=x\cdot\frac{e^{\ln\left(e^{x}+1\right)}-e^{-\ln\left(e^{x}+1\right)}}{e^{\ln\left(e^{x}+1\right)}+e^{-\ln\left(e^{x}+1\right)}}
        var expXPlus1 = Math.Exp(input) + 1.0;
        var lnExpXPlus1 = Math.Log(expXPlus1);
        var expLnExpPlus1 = Math.Exp(lnExpXPlus1);
        var negExpLnExpPlus1 = Math.Exp(-lnExpXPlus1);

        return input * (expLnExpPlus1 - negExpLnExpPlus1) / (expLnExpPlus1 + negExpLnExpPlus1);
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
