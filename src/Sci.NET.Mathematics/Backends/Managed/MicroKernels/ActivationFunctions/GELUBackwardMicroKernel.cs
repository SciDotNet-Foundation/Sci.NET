// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Exceptions;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.ActivationFunctions;

// ReSharper disable once InconsistentNaming
internal class GELUBackwardMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
    where TNumber : unmanaged, INumber<TNumber>, IHyperbolicFunctions<TNumber>, IRootFunctions<TNumber>
{
    [MethodImpl(ImplementationOptions.HotPath)]
    public static bool HasAvx2Implementation()
    {
        return false;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static TNumber ApplyScalar(TNumber input)
    {
        var magicNumber1 = TNumber.CreateChecked(0.3989422804014327);
        var magicNumber2 = TNumber.CreateChecked(0.134145);
        var magicNumber3 = TNumber.CreateChecked(0.044715);
        var two = TNumber.CreateChecked(2);
        var half = TNumber.CreateChecked(0.5);
        var sqrtPiTerm = TNumber.Sqrt(two / TNumber.Pi);

        var xSquared = input * input;
        var xCubed = input * input * input;
        var xPlusMagicXCubed = input + (magicNumber3 * xCubed);

        // \sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)
        var tanhArg = sqrtPiTerm * xPlusMagicXCubed;

        // 0.5\left(1+\tanh\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)\right)
        var halfTanhTerm = half * (TNumber.One + TNumber.Tanh(tanhArg));

        // \operatorname{sech}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechTerm = TNumber.One / TNumber.Cosh(tanhArg);

        // \operatorname{sech}^{2}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechSquared = sechTerm * sechTerm;

        // 0.3989422804014327x\left(1+0.134145x^{2}\right)
        var firstMagicTerm = magicNumber1 * input * (TNumber.One + (magicNumber2 * xSquared));

        return (firstMagicTerm * sechSquared) + halfTanhTerm;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        const float magicNumber1 = 0.3989422804014327f;
        const float magicNumber2 = 0.134145f;
        const float magicNumber3 = 0.044715f;
        const float sqrtPiTerm = 0.79788456080286535587989211986876f;

        var xSquared = input * input;
        var xCubed = input * input * input;
        var xPlusMagicXCubed = input + (magicNumber3 * xCubed);

        // \sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)
        var tanhArg = sqrtPiTerm * xPlusMagicXCubed;

        // 0.5\left(1+\tanh\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)\right)
        var halfTanhTerm = 0.5f * (1.0f + MathF.Tanh(tanhArg));

        // \operatorname{sech}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechTerm = 1.0f / MathF.Cosh(tanhArg);

        // \operatorname{sech}^{2}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechSquared = sechTerm * sechTerm;

        // 0.3989422804014327x\left(1+0.134145x^{2}\right)
        var firstMagicTerm = magicNumber1 * input * (1.0f + (magicNumber2 * xSquared));

        return (firstMagicTerm * sechSquared) + halfTanhTerm;
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        const double magicNumber1 = 0.3989422804014327d;
        const double magicNumber2 = 0.134145d;
        const double magicNumber3 = 0.044715d;
        const double sqrtPiTerm = 0.79788456080286535587989211986876d;

        var xSquared = input * input;
        var xCubed = input * input * input;
        var xPlusMagicXCubed = input + (magicNumber3 * xCubed);

        // \sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)
        var tanhArg = sqrtPiTerm * xPlusMagicXCubed;

        // 0.5\left(1+\tanh\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)\right)
        var halfTanhTerm = 0.5d * (1.0d + Math.Tanh(tanhArg));

        // \operatorname{sech}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechTerm = 1.0d / Math.Cosh(tanhArg);

        // \operatorname{sech}^{2}\left(\sqrt{\frac{2}{\pi}}\left(x+0.044715x^{3}\right)\right)
        var sechSquared = sechTerm * sechTerm;

        // 0.3989422804014327x\left(1+0.134145x^{2}\right)
        var firstMagicTerm = magicNumber1 * input * (1.0d + (magicNumber2 * xSquared));

        return (firstMagicTerm * sechSquared) + halfTanhTerm;
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
