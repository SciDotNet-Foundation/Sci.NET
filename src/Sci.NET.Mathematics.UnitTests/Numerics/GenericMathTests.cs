// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Numerics;

namespace Sci.NET.Mathematics.UnitTests.Numerics;

public class GenericMathTests
{
    [Fact]
    public void IsFloatingPoint_ReturnsTrueForFloatingPointTypes()
    {
        GenericMath.IsFloatingPoint<Half>().Should().BeTrue();
        GenericMath.IsFloatingPoint<BFloat16>().Should().BeTrue();
        GenericMath.IsFloatingPoint<float>().Should().BeTrue();
        GenericMath.IsFloatingPoint<double>().Should().BeTrue();
        GenericMath.IsFloatingPoint<decimal>().Should().BeTrue();
    }

    [Fact]
    public void IsFloatingPoint_ReturnsFalseForNonFloatingPointTypes()
    {
        GenericMath.IsFloatingPoint<sbyte>().Should().BeFalse();
        GenericMath.IsFloatingPoint<byte>().Should().BeFalse();
        GenericMath.IsFloatingPoint<short>().Should().BeFalse();
        GenericMath.IsFloatingPoint<ushort>().Should().BeFalse();
        GenericMath.IsFloatingPoint<int>().Should().BeFalse();
        GenericMath.IsFloatingPoint<uint>().Should().BeFalse();
        GenericMath.IsFloatingPoint<long>().Should().BeFalse();
        GenericMath.IsFloatingPoint<ulong>().Should().BeFalse();
    }

    [Fact]
    public void Epsilon_ReturnsCorrectValueForFloatingPointTypes()
    {
        GenericMath.Epsilon<Half>().Should().Be(Half.Epsilon);
        GenericMath.Epsilon<BFloat16>().Should().Be(BFloat16.Epsilon);
        GenericMath.Epsilon<float>().Should().Be(float.Epsilon);
        GenericMath.Epsilon<double>().Should().Be(double.Epsilon);
    }

    [Fact]
    public void Epsilon_ReturnsCorrectValueForNonFloatingPointTypes()
    {
        GenericMath.Epsilon<sbyte>().Should().Be(1);
        GenericMath.Epsilon<byte>().Should().Be(1);
        GenericMath.Epsilon<short>().Should().Be(1);
        GenericMath.Epsilon<ushort>().Should().Be(1);
        GenericMath.Epsilon<int>().Should().Be(1);
        GenericMath.Epsilon<uint>().Should().Be(1);
        GenericMath.Epsilon<long>().Should().Be(1);
        GenericMath.Epsilon<ulong>().Should().Be(1);
    }

    [Fact]
    public void IsSigned_ReturnsCorrectValues()
    {
        GenericMath.IsSigned<BFloat16>().Should().BeTrue();
        GenericMath.IsSigned<Half>().Should().BeTrue();
        GenericMath.IsSigned<float>().Should().BeTrue();
        GenericMath.IsSigned<double>().Should().BeTrue();
        GenericMath.IsSigned<decimal>().Should().BeTrue();
        GenericMath.IsSigned<byte>().Should().BeFalse();
        GenericMath.IsSigned<sbyte>().Should().BeTrue();
        GenericMath.IsSigned<ushort>().Should().BeFalse();
        GenericMath.IsSigned<short>().Should().BeTrue();
        GenericMath.IsSigned<uint>().Should().BeFalse();
        GenericMath.IsSigned<int>().Should().BeTrue();
        GenericMath.IsSigned<ulong>().Should().BeFalse();
        GenericMath.IsSigned<long>().Should().BeTrue();
    }

    [Theory]
    [InlineData(81)]
    [InlineData(64)]
    [InlineData(49)]
    [InlineData(36)]
    [InlineData(25)]
    [InlineData(16)]
    [InlineData(9)]
    [InlineData(4)]
    [InlineData(3)]
    [InlineData(2)]
    [InlineData(0)]
    public void Sqrt_GivenIntegerInputs_ReturnsCorrectValue(int value)
    {
        var bf16Input = BFloat16.CreateChecked(value);
        var fp16Input = Half.CreateChecked(value);
        var fp32Input = float.CreateChecked(value);
        var fp64Input = double.CreateChecked(value);
        var s8Input = sbyte.CreateChecked(value);
        var u8Input = byte.CreateChecked(value);
        var s16Input = short.CreateChecked(value);
        var u16Input = ushort.CreateChecked(value);
        var s32Input = int.CreateChecked(value);
        var u32Input = uint.CreateChecked(value);
        var s64Input = long.CreateChecked(value);
        var u64Input = ulong.CreateChecked(value);

        var bf16Expected = BFloat16.Sqrt(bf16Input);
        var fp16Expected = Half.Sqrt(fp16Input);
        var fp32Expected = float.Sqrt(fp32Input);
        var fp64Expected = double.Sqrt(fp64Input);
        var s8IExpected = (sbyte)Math.Sqrt(s8Input);
        var u8IExpected = (byte)Math.Sqrt(u8Input);
        var s16Expected = (short)Math.Sqrt(s16Input);
        var u16Expected = (ushort)Math.Sqrt(u16Input);
        var s32Expected = (int)Math.Sqrt(s32Input);
        var u32Expected = (uint)Math.Sqrt(u32Input);
        var s64Expected = (long)Math.Sqrt(s64Input);
        var u64Expected = (ulong)Math.Sqrt(u64Input);

        GenericMath.Sqrt(bf16Input).Should().Be(bf16Expected);
        GenericMath.Sqrt(fp16Input).Should().Be(fp16Expected);
        GenericMath.Sqrt(fp32Input).Should().Be(fp32Expected);
        GenericMath.Sqrt(fp64Input).Should().Be(fp64Expected);
        GenericMath.Sqrt(s8Input).Should().Be(s8IExpected);
        GenericMath.Sqrt(u8Input).Should().Be(u8IExpected);
        GenericMath.Sqrt(s16Input).Should().Be(s16Expected);
        GenericMath.Sqrt(u16Input).Should().Be(u16Expected);
        GenericMath.Sqrt(s32Input).Should().Be(s32Expected);
        GenericMath.Sqrt(u32Input).Should().Be(u32Expected);
        GenericMath.Sqrt(s64Input).Should().Be(s64Expected);
        GenericMath.Sqrt(u64Input).Should().Be(u64Expected);
    }
}
