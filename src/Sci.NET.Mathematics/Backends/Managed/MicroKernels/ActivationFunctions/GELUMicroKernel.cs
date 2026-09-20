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
internal class GELUMicroKernel<TNumber> : IUnaryOperation<TNumber>, IUnaryOperationAvx2
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
        var two = TNumber.CreateChecked(2);
        var magicNumber = TNumber.CreateChecked(0.044715);
        var sqrtPiTerm = TNumber.Sqrt(two / TNumber.Pi);

        var x = input;
        var halfX = x / two;
        var xCubed = x * x * x;
        var tanhArg = sqrtPiTerm * (x + (magicNumber * xCubed));

        return halfX * (TNumber.One + TNumber.Tanh(tanhArg));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static float ApplyScalarFp32(float input)
    {
        const float magicNumber = 0.044715f;
        const float sqrtPiTerm = 0.79788456080286535587989211986876f;

        var x = input;
        var halfX = x / 2.0f;
        var xCubed = x * x * x;
        var tanhArg = sqrtPiTerm * (x + (magicNumber * xCubed));

        return halfX * (1.0f + MathF.Tanh(tanhArg));
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    public static double ApplyScalarFp64(double input)
    {
        const double magicNumber = 0.044715;
        const double sqrtPiTerm = 0.79788456080286535587989211986876;

        var x = input;
        var halfX = x / 2.0;
        var xCubed = x * x * x;
        var tanhArg = sqrtPiTerm * (x + (magicNumber * xCubed));

        return halfX * (1.0 + Math.Tanh(tanhArg));
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
