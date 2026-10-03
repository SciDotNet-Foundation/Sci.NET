// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Random;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

namespace Sci.NET.Mathematics.UnitTests.Random.PrngTests;

public class HeNormalShould : PrngTestBase
{
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void NotBeEqual_WhenRunTwice_BFloat16(int fanIn)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var aMemory = new SystemMemoryBlock<BFloat16>(fanIn);
            using var bMemory = new SystemMemoryBlock<BFloat16>(fanIn);
            var prng = new Prng(seed);

            // Act
            prng.FillHeNormal(aMemory, fanIn);
            prng.FillHeNormal(bMemory, fanIn);

            // Assert
            var aArray = aMemory.ToArray();
            var bArray = bMemory.ToArray();

            aArray.Should().NotBeEquivalentTo(bArray);
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void NotBeEqual_WhenRunTwice_Float16(int fanIn)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var aMemory = new SystemMemoryBlock<Half>(fanIn);
            using var bMemory = new SystemMemoryBlock<Half>(fanIn);
            var prng = new Prng(seed);

            // Act
            prng.FillHeNormal(aMemory, fanIn);
            prng.FillHeNormal(bMemory, fanIn);

            // Assert
            var aArray = aMemory.ToArray();
            var bArray = bMemory.ToArray();

            aArray.Should().NotBeEquivalentTo(bArray);
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void NotBeEqual_WhenRunTwice_Float32(int fanIn)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var aMemory = new SystemMemoryBlock<float>(fanIn);
            using var bMemory = new SystemMemoryBlock<float>(fanIn);
            var prng = new Prng(seed);

            // Act
            prng.FillHeNormal(aMemory, fanIn);
            prng.FillHeNormal(bMemory, fanIn);

            // Assert
            var aArray = aMemory.ToArray();
            var bArray = bMemory.ToArray();

            aArray.Should().NotBeEquivalentTo(bArray);
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void NotBeEqual_WhenRunTwice_Float64(int fanIn)
    {
        foreach (var seed in Seeds)
        {
            // Arrange
            using var aMemory = new SystemMemoryBlock<double>(fanIn);
            using var bMemory = new SystemMemoryBlock<double>(fanIn);
            var prng = new Prng(seed);

            // Act
            prng.FillHeNormal(aMemory, fanIn);
            prng.FillHeNormal(bMemory, fanIn);

            // Assert
            var aArray = aMemory.ToArray();
            var bArray = bMemory.ToArray();

            aArray.Should().NotBeEquivalentTo(bArray);
        }
    }
}