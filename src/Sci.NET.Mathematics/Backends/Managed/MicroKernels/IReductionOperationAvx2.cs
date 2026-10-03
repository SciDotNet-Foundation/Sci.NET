// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Runtime.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels;

/// <summary>
/// Defines an AVX reduction operation micro-kernel.
/// </summary>
public interface IReductionOperationAvx2
{
    /// <summary>
    /// Gets the identity vector for AVX 256-bit float32 reduction operations.
    /// </summary>
    public static abstract Vector256<float> Avx256Fp32Identity { get; }

    /// <summary>
    /// Gets the identity vector for AVX 256-bit float64 reduction operations.
    /// </summary>
    public static abstract Vector256<double> Avx256Fp64Identity { get; }

    /// <summary>
    /// Determines whether AVX is supported on the current machine.
    /// </summary>
    /// <returns><see langword="true"/> if AVX is supported; otherwise, <see langword="false"/>.</returns>
    public static abstract bool HasAvx2Implementation();

    /// <summary>
    /// Accumulates two AVX 256-bit float32 vectors.
    /// </summary>
    /// <param name="current">The current accumulated vector.</param>
    /// <param name="values">The vector to accumulate.</param>
    /// <returns>The accumulated vector.</returns>
    public static abstract Vector256<float> AccumulateAvxFp32(Vector256<float> current, Vector256<float> values);

    /// <summary>
    /// Accumulates two AVX 256-bit float64 vectors.
    /// </summary>
    /// <param name="current">The current accumulated vector.</param>
    /// <param name="values">The vector to accumulate.</param>
    /// <returns>The accumulated vector.</returns>
    public static abstract Vector256<double> AccumulateAvxFp64(Vector256<double> current, Vector256<double> values);

    /// <summary>
    /// Horizontally reduces an AVX 256-bit float32 vector to a single float32 value.
    /// </summary>
    /// <param name="vector">The vector to reduce.</param>
    /// <returns>The reduced float32 value.</returns>
    public static abstract float HorizontalReduceAvxFp32(Vector256<float> vector);

    /// <summary>
    /// Horizontally reduces an AVX 256-bit float64 vector to a single float64 value.
    /// </summary>
    /// <param name="vector">The vector to reduce.</param>
    /// <returns>The reduced float64 value.</returns>
    public static abstract double HorizontalReduceAvxFp64(Vector256<double> vector);
}