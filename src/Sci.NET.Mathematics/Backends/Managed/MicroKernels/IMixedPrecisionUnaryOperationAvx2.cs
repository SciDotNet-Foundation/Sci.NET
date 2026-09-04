// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Runtime.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels;

/// <summary>
/// An interface for AVX2 mixed precision operations.
/// </summary>
public interface IMixedPrecisionUnaryOperationAvx2 : IMixedPrecisionUnaryOperationConcreteScalar
{
    /// <summary>
    /// Determines whether AVX/FMA is supported on the current machine.
    /// </summary>
    /// <returns>A boolean indicating whether AVX is supported.</returns>
    public static abstract bool HasAvx2Implementation();

    /// <summary>
    /// Invokes the vectorized operation on a Vector128 float input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public static abstract Vector256<double> ApplyAvxFp32Fp64(Vector128<float> input);

    /// <summary>
    /// Invokes the vectorized operation on a Vector256 double input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public static abstract Vector128<float> ApplyAvxFp64Fp32(Vector256<double> input);
}