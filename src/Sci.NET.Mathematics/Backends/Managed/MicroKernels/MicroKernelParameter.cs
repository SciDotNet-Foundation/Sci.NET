// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.Intrinsics;
using Sci.NET.Mathematics.Comparison;
using Sci.NET.Mathematics.Numerics;

namespace Sci.NET.Mathematics.Backends.Managed.MicroKernels;

/// <summary>
/// A parameter container for micro-kernel operations.
/// </summary>
/// <typeparam name="TNumber">The numeric type.</typeparam>
public readonly struct MicroKernelParameter<TNumber> : IValueEquatable<MicroKernelParameter<TNumber>>
    where TNumber : unmanaged, INumber<TNumber>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicroKernelParameter{TNumber}"/> struct with the specified scalar value.
    /// </summary>
    /// <param name="value">The scalar value.</param>
    public MicroKernelParameter(TNumber value)
    {
        ScalarValue = value;

        if (GenericMath.IsFloatingPoint<TNumber>())
        {
            ScalarFp32Value = float.CreateChecked(ScalarValue);
            ScalarFp64Value = double.CreateChecked(ScalarValue);
            Vector256ValueFp32 = Vector256.Create(ScalarFp32Value);
            Vector256ValueFp64 = Vector256.Create(ScalarFp64Value);
        }
        else
        {
            ScalarFp32Value = 0;
            ScalarFp64Value = 0;
        }
    }

    /// <summary>
    /// Gets the scalar value.
    /// </summary>
    public TNumber ScalarValue { get; }

    /// <summary>
    /// Gets the scalar float (FP32) value.
    /// </summary>
    public float ScalarFp32Value { get; }

    /// <summary>
    /// Gets the scalar double (FP64) value.
    /// </summary>
    public double ScalarFp64Value { get; }

    /// <summary>
    /// Gets <see cref="Vector256{T}"/> float (FP32) value.
    /// </summary>
    public Vector256<float> Vector256ValueFp32 { get; }

    /// <summary>
    /// Gets the <see cref="Vector256{T}"/> float (FP64) value.
    /// </summary>
    public Vector256<double> Vector256ValueFp64 { get; }

    /// <summary>
    /// Implicitly converts a TNumber value to a <see cref="MicroKernelParameter{TNumber}"/>.
    /// </summary>
    /// <param name="value">The TNumber value.</param>
    /// <returns>The corresponding <see cref="MicroKernelParameter{TNumber}"/> instance.</returns>
    public static implicit operator MicroKernelParameter<TNumber>(TNumber value)
    {
        return new(value);
    }

    /// <inheritdoc />
    public static bool operator ==(MicroKernelParameter<TNumber> left, MicroKernelParameter<TNumber> right) =>
        left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(MicroKernelParameter<TNumber> left, MicroKernelParameter<TNumber> right) =>
        !left.Equals(right);

    /// <inheritdoc />
    public bool Equals(MicroKernelParameter<TNumber> other)
    {
        return other.ScalarValue.Equals(ScalarValue);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is MicroKernelParameter<TNumber> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(ScalarValue);
    }
}