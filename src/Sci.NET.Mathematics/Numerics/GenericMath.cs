// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sci.NET.Mathematics.Numerics;

/// <summary>
/// A helper class for generic math operations.
/// </summary>
public static class GenericMath
{
    /// <summary>
    /// Determines if the number type is floating point.
    /// </summary>
    /// <typeparam name="TNumber">The number type to test.</typeparam>
    /// <returns><see langword="true"/> if the number is a floating point type, else, <see langword="false"/>.</returns>
    public static bool IsFloatingPoint<TNumber>()
        where TNumber : unmanaged, INumber<TNumber>
    {
        return Epsilon<TNumber>() != TNumber.One;
    }

    /// <summary>
    /// Determines if the number type is signed.
    /// </summary>
    /// <typeparam name="TNumber">The number type to test.</typeparam>
    /// <returns><see langword="true"/> if the number is a signed type, else, <see langword="false"/>.</returns>
    public static bool IsSigned<TNumber>()
        where TNumber : unmanaged, INumber<TNumber>
    {
        return unchecked(TNumber.Zero - TNumber.One) < TNumber.Zero;
    }

    /// <summary>
    /// Gets the machine epsilon for the specified number type.
    /// </summary>
    /// <typeparam name="TNumber">The number type to get the epsilon for.</typeparam>
    /// <returns>The machine epsilon for the specified number type.</returns>
    public static unsafe TNumber Epsilon<TNumber>()
        where TNumber : unmanaged, INumber<TNumber>
    {
        var instance = TNumber.Zero;

        Unsafe.Write(&instance, 0x01);

        return instance;
    }

    /// <summary>
    /// Finds the square root of the provided number.
    /// </summary>
    /// <param name="number">The number to find the square root of.</param>
    /// <typeparam name="TNumber">The number type.</typeparam>
    /// <returns>The square root of the provided number.</returns>
    /// <exception cref="NotSupportedException">Thrown if the number type is not supported.</exception>
    public static TNumber Sqrt<TNumber>(TNumber number)
        where TNumber : unmanaged
    {
        return number switch
        {
            float f => Unsafe.BitCast<float, TNumber>(MathF.Sqrt(f)),
            double d => Unsafe.BitCast<double, TNumber>(Math.Sqrt(d)),
            BFloat16 b => Unsafe.BitCast<BFloat16, TNumber>(BFloat16.Sqrt(b)),
            Half h => Unsafe.BitCast<Half, TNumber>(Half.Sqrt(h)),
            Complex c => Unsafe.BitCast<Complex, TNumber>(Complex.Sqrt(c)),
            byte b => Unsafe.BitCast<byte, TNumber>((byte)MathF.Sqrt(b)),
            short s => Unsafe.BitCast<short, TNumber>((short)MathF.Sqrt(s)),
            int i => Unsafe.BitCast<int, TNumber>((int)Math.Sqrt(i)),
            long l => Unsafe.BitCast<long, TNumber>((long)Math.Sqrt(l)),
            nint n => Unsafe.BitCast<nint, TNumber>((IntPtr)Math.Sqrt(n)),
            nuint n => Unsafe.BitCast<nuint, TNumber>((UIntPtr)Math.Sqrt(n)),
            sbyte s => Unsafe.BitCast<sbyte, TNumber>((sbyte)MathF.Sqrt(s)),
            ushort u => Unsafe.BitCast<ushort, TNumber>((ushort)MathF.Sqrt(u)),
            uint u => Unsafe.BitCast<uint, TNumber>((uint)Math.Sqrt(u)),
            ulong u => Unsafe.BitCast<ulong, TNumber>((ulong)Math.Sqrt(u)),
            _ => throw new NotSupportedException("Type not supported for square root."),
        };
    }

    /// <summary>
    /// Finds a scaled epsilon for the specified number type. If the number type is a floating point type, the epsilon is multiplied by the specified multiplier.
    /// </summary>
    /// <typeparam name="TNumber">The number type to find the epsilon for.</typeparam>
    /// <param name="multiplier">The multiplier to use for the epsilon.</param>
    /// <returns>A small epsilon for the specified number type.</returns>
    public static TNumber ScaledEpsilon<TNumber>(int multiplier)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var epsilon = Epsilon<TNumber>();

        if (IsFloatingPoint<TNumber>())
        {
            epsilon *= TNumber.CreateChecked(multiplier);
        }

        return epsilon;
    }

    /// <summary>
    /// Gets the maximum value for the specified number type.
    /// </summary>
    /// <typeparam name="TNumber">The number type to get the maximum value for.</typeparam>
    /// <returns>The maximum value for the specified number type.</returns>
    /// <exception cref="NotSupportedException">Thrown if the number type is not supported.</exception>
    public static TNumber MaxValue<TNumber>()
        where TNumber : unmanaged, INumber<TNumber>
    {
        return TNumber.Zero switch
        {
            sbyte => Unsafe.BitCast<sbyte, TNumber>(sbyte.MaxValue),
            byte => Unsafe.BitCast<byte, TNumber>(byte.MaxValue),
            short => Unsafe.BitCast<short, TNumber>(short.MaxValue),
            ushort => Unsafe.BitCast<ushort, TNumber>(ushort.MaxValue),
            int => Unsafe.BitCast<int, TNumber>(int.MaxValue),
            uint => Unsafe.BitCast<uint, TNumber>(uint.MaxValue),
            long => Unsafe.BitCast<long, TNumber>(long.MaxValue),
            ulong => Unsafe.BitCast<ulong, TNumber>(ulong.MaxValue),
            float => Unsafe.BitCast<float, TNumber>(float.MaxValue),
            double => Unsafe.BitCast<double, TNumber>(double.MaxValue),
            BFloat16 => Unsafe.BitCast<BFloat16, TNumber>(BFloat16.MaxValue),
            Half => Unsafe.BitCast<Half, TNumber>(Half.MaxValue),
            _ => throw new NotSupportedException("Type not supported for MaxValue."),
        };
    }

    /// <summary>
    /// Gets the minimum value for the specified number type.
    /// </summary>
    /// <typeparam name="TNumber">The number type to get the minimum value for.</typeparam>
    /// <returns>The minimum value for the specified number type.</returns>
    /// <exception cref="NotSupportedException">Thrown if the number type is not supported.</exception>
    public static TNumber MinValue<TNumber>()
        where TNumber : unmanaged, INumber<TNumber>
    {
        return TNumber.Zero switch
        {
            sbyte => Unsafe.BitCast<sbyte, TNumber>(sbyte.MinValue),
            byte => Unsafe.BitCast<byte, TNumber>(byte.MinValue),
            short => Unsafe.BitCast<short, TNumber>(short.MinValue),
            ushort => Unsafe.BitCast<ushort, TNumber>(ushort.MinValue),
            int => Unsafe.BitCast<int, TNumber>(int.MinValue),
            uint => Unsafe.BitCast<uint, TNumber>(uint.MinValue),
            long => Unsafe.BitCast<long, TNumber>(long.MinValue),
            ulong => Unsafe.BitCast<ulong, TNumber>(ulong.MinValue),
            float => Unsafe.BitCast<float, TNumber>(float.MinValue),
            double => Unsafe.BitCast<double, TNumber>(double.MinValue),
            BFloat16 => Unsafe.BitCast<BFloat16, TNumber>(BFloat16.MinValue),
            Half => Unsafe.BitCast<Half, TNumber>(Half.MinValue),
            _ => throw new NotSupportedException("Type not supported for MaxValue."),
        };
    }
}
