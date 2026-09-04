// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Runtime.Intrinsics.X86;

namespace Sci.NET.Mathematics.Intrinsics;

/// <summary>
/// A helper class for SIMD intrinsics.
/// </summary>
[PublicAPI]
public static class IntrinsicsHelper
{
    /// <summary>
    /// The element count of an AVX vector for single-precision floating-point numbers (FP32).
    /// </summary>
    public const int AvxVectorCountFp32 = 8;

    /// <summary>
    /// The element count of an AVX vector for double-precision floating-point numbers (FP64).
    /// </summary>
    public const int AvxVectorCountFp64 = 4;

#pragma warning disable CA1810
    static IntrinsicsHelper()
#pragma warning restore CA1810
    {
        EnableSimd();
    }

    /// <summary>
    /// Gets the available SIMD instruction sets on the current platform.
    /// </summary>
    public static SimdInstructionSet AvailableInstructionSets { get; private set; }

    /// <summary>
    /// Gets the required alignment for SIMD operations based on the available instruction sets.
    /// </summary>
    /// <returns>The required alignment as a <see cref="UIntPtr"/>.</returns>
    public static UIntPtr CalculateRequiredAlignment()
    {
        if ((AvailableInstructionSets & SimdInstructionSet.Avx512F) != 0)
        {
            return new UIntPtr(64);
        }

        if ((AvailableInstructionSets & SimdInstructionSet.Avx2) != 0 ||
            (AvailableInstructionSets & SimdInstructionSet.Avx) != 0)
        {
            return new UIntPtr(32);
        }

        if ((AvailableInstructionSets & SimdInstructionSet.Sse41) != 0 ||
            (AvailableInstructionSets & SimdInstructionSet.Sse2) != 0 ||
            (AvailableInstructionSets & SimdInstructionSet.Sse2) != 0 ||
            (AvailableInstructionSets & SimdInstructionSet.Sse) != 0)
        {
            return new UIntPtr(16);
        }

        return new UIntPtr(1);
    }

    /// <summary>
    /// Disables SIMD intrinsics, setting the available instruction sets to <see cref="SimdInstructionSet.None"/>.
    /// </summary>
    public static void DisableSimd()
    {
        AvailableInstructionSets = SimdInstructionSet.None;
    }

    /// <summary>
    /// Enables SIMD intrinsics based on the current platform's capabilities.
    /// </summary>
    public static void EnableSimd()
    {
        AvailableInstructionSets = GetAvailableInstructionSets();
    }

    /// <summary>
    /// Checks if AVX2 instruction set is supported, including FMA.
    /// </summary>
    /// <returns>A value indicating whether AVX2 is supported.</returns>
    /// <remarks>
    /// We require FMA support alongside AVX2 for our optimizations.
    /// </remarks>
    public static bool IsAvx2Supported()
    {
        return (AvailableInstructionSets & SimdInstructionSet.Avx2) != 0 &&
               (AvailableInstructionSets & SimdInstructionSet.Fma) != 0;
    }

    private static SimdInstructionSet GetAvailableInstructionSets()
    {
        var instructionSets = SimdInstructionSet.None;

        if (Sse.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Sse;
        }

        if (Sse2.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Sse2;
        }

        if (Sse3.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Sse3;
        }

        if (Sse41.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Sse41;
        }

        if (Avx.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Avx;
        }

        if (Avx2.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Avx2;
        }

        if (Avx512F.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Avx512F;
        }

        if (Fma.IsSupported)
        {
            instructionSets |= SimdInstructionSet.Fma;
        }

        return instructionSets;
    }
}