// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels;

/// <summary>
/// An interface exposing concretely typed methods for mixed precision operations.
/// </summary>
public interface IMixedPrecisionUnaryOperationConcreteScalar
{
    /// <summary>
    /// Applies the operation to a float input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public static abstract double ApplyScalarFp32Fp64(float input);

    /// <summary>
    /// Applies the operation to a double input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The result of the operation.</returns>
    public static abstract float ApplyScalarFp64Fp32(double input);
}