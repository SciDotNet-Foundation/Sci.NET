// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Memory;

namespace Sci.NET.Mathematics.UnitTests.Memory.SystemMemoryBlock;

public class FillBytesShould
{
    [Fact]
    public void FillBytes()
    {
        // Arrange
        var block = new SystemMemoryBlock<int>(10);
        byte[] buffer = BitConverter.IsLittleEndian
            ? [1, 0, 0, 0]
            : [0, 0, 0, 1];

        // Act
        block.FillBytes(4, buffer, 4);
        var result = block.ToArray(); // Potential scope leakage

        // Assert
        result.Should().BeEquivalentTo([0, 0, 0, 0, 1, 0, 0, 0, 0, 0]);
    }
}
