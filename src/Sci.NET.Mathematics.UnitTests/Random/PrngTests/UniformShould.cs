// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Random;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

namespace Sci.NET.Mathematics.UnitTests.Random.PrngTests;

public class UniformShould : PrngTestBase
{
    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_BFloat16(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<BFloat16>(10);
        using var bMemory = new SystemMemoryBlock<BFloat16>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0.0f, 1.0f);
        Prng.Instance.FillUniform(bMemory, 0.0f, 1.0f);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_Float16(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<Half>(10);
        using var bMemory = new SystemMemoryBlock<Half>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, (Half)0.0f, (Half)1.0f);
        Prng.Instance.FillUniform(bMemory, (Half)0.0f, (Half)1.0f);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_Float32(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<float>(10);
        using var bMemory = new SystemMemoryBlock<float>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0.0f, 1.0f);
        Prng.Instance.FillUniform(bMemory, 0.0f, 1.0f);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_Float64(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<double>(10);
        using var bMemory = new SystemMemoryBlock<double>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0.0d, 1.0d);
        Prng.Instance.FillUniform(bMemory, 0.0d, 1.0d);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatUInt8(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<byte>(10);
        using var bMemory = new SystemMemoryBlock<byte>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatInt8(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<sbyte>(10);
        using var bMemory = new SystemMemoryBlock<sbyte>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatUInt16(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<ushort>(10);
        using var bMemory = new SystemMemoryBlock<ushort>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatInt16(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<short>(10);
        using var bMemory = new SystemMemoryBlock<short>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatUInt32(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<uint>(10);
        using var bMemory = new SystemMemoryBlock<uint>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatInt32(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<int>(10);
        using var bMemory = new SystemMemoryBlock<int>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatUInt64(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<ulong>(10);
        using var bMemory = new SystemMemoryBlock<ulong>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_FloatInt64(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<long>(10);
        using var bMemory = new SystemMemoryBlock<long>(10);

        Prng.Instance.SetSeed(seed);

        // Act
        Prng.Instance.FillUniform(aMemory, 0, 100);
        Prng.Instance.FillUniform(bMemory, 0, 100);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [InlineData(-1.0f, 1.0f)]
    [InlineData(-1000.0f, 100.0f)]
    [InlineData(0f, 10000.0f)]
    public void BeInRange_BFloat16(BFloat16 min, BFloat16 max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<BFloat16>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Theory]
    [InlineData(-1.0f, 1.0f)]
    [InlineData(-1000.0f, 100.0f)]
    [InlineData(0f, 10000.0f)]
    public void BeInRange_Float16(Half min, Half max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<Half>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Theory]
    [InlineData(-1.0f, 1.0f)]
    [InlineData(-1000.0f, 100.0f)]
    [InlineData(0f, 10000.0f)]
    public void BeInRange_Float32(float min, float max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<float>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Theory]
    [InlineData(-1.0d, 1.0d)]
    [InlineData(-1000.0d, 100.0d)]
    [InlineData(0d, 10000.0d)]
    public void BeInRange_Float64(double min, double max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<double>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(10, 50)]
    [InlineData(100, 240)]
    public void BeInRange_UInt8(byte min, byte max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<byte>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Theory]
    [InlineData(-100, 100)]
    [InlineData(-50, 50)]
    [InlineData(0, 100)]
    public void BeInRange_Int8(sbyte min, sbyte max)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<sbyte>(1000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var items = memory.ToArray();

            items.Should().AllBeInRange(min, max);
        }
    }

    [Fact]
    public void ShouldBeUniform_Int8()
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<sbyte>(10_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, sbyte.MinValue, sbyte.MaxValue);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[byte.MaxValue];

            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = (sbyte)allItems.Count(x => x == i - 128);
            }

            var expected = memory.Length / buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 293.24783508;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Fact]
    public void ShouldBeUniform_UInt8()
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var memory = new SystemMemoryBlock<byte>(10_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, byte.MinValue, byte.MaxValue);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[byte.MaxValue];

            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = (sbyte)allItems.Count(x => x == i);
            }

            var expected = memory.Length / buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 293.24783508;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(-32768)]
    [InlineData(-10000)]
    [InlineData(-500)]
    [InlineData(500)]
    [InlineData(10000)]
    [InlineData(30000)]
    public void ShouldBeUniform_Int16(short min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const short range = 1000;
            var max = (short)(min + range);
            using var memory = new SystemMemoryBlock<short>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = (short)allItems.Count(x => x == min + i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(500)]
    [InlineData(10000)]
    [InlineData(30000)]
    [InlineData(60000)]
    public void ShouldBeUniform_UInt16(ushort min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const ushort range = 1000;
            var max = (ushort)(min + range);
            using var memory = new SystemMemoryBlock<ushort>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = (ushort)allItems.Count(x => x == min + i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(-2147483648)]
    [InlineData(-100000)]
    [InlineData(0)]
    [InlineData(100000)]
    [InlineData(2147482648)]
    public void ShouldBeUniform_Int32(int min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const int range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<int>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = allItems.Count(x => x == min + i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100000)]
    [InlineData(7482648)]
    [InlineData(2147482648)]
    [InlineData(4294966294)]
    public void ShouldBeUniform_UInt32(uint min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const uint range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<uint>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = allItems.Count(x => x == min + i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(-9223372036854775808)]
    [InlineData(-684654657)]
    [InlineData(0)]
    [InlineData(2147482648)]
    [InlineData(9223372036854773807)]
    public void ShouldBeUniform_Int64(long min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const long range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<long>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = allItems.Count(x => x == min + i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4775808)]
    [InlineData(684654657)]
    [InlineData(2147482648)]
    [InlineData(18446744073709550515)]
    public void ShouldBeUniform_UInt64(ulong min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const long range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<ulong>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var allItems = memory.ToArray();
            var buckets = new int[range];

            for (var i = 0; i < range; i++)
            {
                buckets[i] = allItems.Count(x => x == min + (ulong)i);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(-10000)]
    [InlineData(-125f)]
    [InlineData(0.0f)]
    [InlineData(125)]
    [InlineData(1000)]
    public void ShouldBeUniform_Float32(float min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const long range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<float>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var width = max - min;
            var allItems = memory.ToArray();
            var buckets = new int[range];
            var normalized = allItems.Select(x => (x - min) / width).ToArray();

            for (var i = 0; i < range; i++)
            {
                var low = i / (double)range;
                var high = (i + 1) / (double)range;
                buckets[i] = normalized.Count(x => x >= low && x < high);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }

    [Theory]
    [InlineData(-10000)]
    [InlineData(-125f)]
    [InlineData(0.0f)]
    [InlineData(125)]
    [InlineData(1000)]
    public void ShouldBeUniform_Float64(double min)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            const long range = 1000;
            var max = min + range;
            using var memory = new SystemMemoryBlock<double>(20_000);
            var prng = new Prng(seed);

            // Act
            prng.FillUniform(memory, min, max);

            // Assert
            var width = max - min;
            var allItems = memory.ToArray();
            var buckets = new int[range];
            var normalized = allItems.Select(x => (x - min) / width).ToArray();

            for (var i = 0; i < range; i++)
            {
                var low = i / (double)range;
                var high = (i + 1) / (double)range;
                buckets[i] = normalized.Count(x => x >= low && x < high);
            }

            var expected = memory.Length / (double)buckets.Length;
            var chi2 = buckets.Sum(b => Math.Pow(b - expected, 2) / expected);
            const double chi2Critical05 = 1073.64265066;

            chi2.Should().BeLessThan(chi2Critical05);
        }
    }
}