// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels.Casting;

[SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter", Justification = "By design")]
internal class CastTruncatingMicroKernel<TIn, TOut> : IMixedPrecisionUnaryOperation<TIn, TOut>
    where TIn : unmanaged, INumber<TIn>
    where TOut : unmanaged, INumber<TOut>
{
    /// <inheritdoc />
    public static TOut ApplyScalar(TIn input)
    {
        return TOut.CreateTruncating(input);
    }
}