// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Linq;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Random;

namespace Sci.NET.Mathematics.UnitTests.Random.PrngTests;

public class NormalShould : PrngTestBase
{
    [Theory]
    [MemberData(nameof(Seeds))]
    public void NotBeEqual_WhenRunTwice_BFloat16(ulong seed)
    {
        // Arrange
        using var aMemory = new SystemMemoryBlock<BFloat16>(10);
        using var bMemory = new SystemMemoryBlock<BFloat16>(10);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(aMemory, 0.0f, 1.0f);
        prng.FillNormal(bMemory, 0.0f, 1.0f);

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
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(aMemory, (Half)0.0f, (Half)1.0f);
        prng.FillNormal(bMemory, (Half)0.0f, (Half)1.0f);

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
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(aMemory, 0.0f, 1.0f);
        prng.FillNormal(bMemory, 0.0f, 1.0f);

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
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(aMemory, 0.0f, 1.0f);
        prng.FillNormal(bMemory, 0.0f, 1.0f);

        // Assert
        var aArray = aMemory.ToArray();
        var bArray = bMemory.ToArray();

        aArray.Should().NotBeEquivalentTo(bArray);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void FillsAllValues_Float16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<Half>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, (Half)0.0f, (Half)1.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().NotContain(Half.NaN);
        array.Should().NotContain(Half.PositiveInfinity);
        array.Should().NotContain(Half.NegativeInfinity);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void FillsAllValues_BFloat16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<BFloat16>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0f, 1.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().NotContain(BFloat16.NaN);
        array.Should().NotContain(BFloat16.PositiveInfinity);
        array.Should().NotContain(BFloat16.NegativeInfinity);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void FillsAllValues_Float32(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<float>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0f, 1.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().NotContain(float.NaN);
        array.Should().NotContain(float.PositiveInfinity);
        array.Should().NotContain(float.NegativeInfinity);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void FillsAllValues_Float64(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<double>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0f, 1.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().NotContain(double.NaN);
        array.Should().NotContain(double.PositiveInfinity);
        array.Should().NotContain(double.NegativeInfinity);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void IsConstantMeanWhenZeroStd_Float16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<Half>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, (Half)5.0f, (Half)0.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().OnlyContain(static x => x == (Half)5.0f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void IsConstantMeanWhenZeroStd_BFloat16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<BFloat16>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 5.0f, 0.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().OnlyContain(static x => x == 5.0f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void IsConstantMeanWhenZeroStd_Float32(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<float>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 5.0f, 0.0f);

        // Assert
        var array = memory.ToArray();

        array.Should().OnlyContain(static x => x == 5.0f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void IsConstantMeanWhenZeroStd_Float64(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<double>(1000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 5.0, 0.0);

        // Assert
        var array = memory.ToArray();

        array.Should().OnlyContain(static x => x == 5.0);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void HasApproximatelyAccurateMean_BFloat16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<BFloat16>(20_000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0f, 1.0f);

        // Assert
        var array = memory.ToArray();
        var mean = (float)array.Sum() / memory.Length;

        mean.Should().BeApproximately(0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void HasApproximatelyAccurateMean_Float16(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<Half>(20_000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, (Half)0.0f, (Half)1.0f);

        // Assert
        var array = memory.ToArray();
        var mean = (float)array.Sum() / memory.Length;

        mean.Should().BeApproximately(0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void HasApproximatelyAccurateMean_Float32(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<float>(20_000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0f, 1.0f);

        // Assert
        var array = memory.ToArray();
        var mean = array.Sum() / memory.Length;

        mean.Should().BeApproximately(0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void HasApproximatelyAccurateMean_Float64(ulong seed)
    {
        // Arrange
        using var memory = new SystemMemoryBlock<double>(20_000);
        var prng = new Prng(seed);

        // Act
        prng.FillNormal(memory, 0.0d, 1.0d);

        // Assert
        var array = memory.ToArray();
        var mean = array.Sum() / memory.Length;

        mean.Should().BeApproximately(0.0d, 0.01d);
    }
}