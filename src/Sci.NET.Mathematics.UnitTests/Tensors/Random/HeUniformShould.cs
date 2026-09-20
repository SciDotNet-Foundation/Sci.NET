// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Random;

public class HeUniformShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectRange_Float16(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedMax = Math.Sqrt(6.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeUniform<Half>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        tensor.Should().BeInRange((Half)(-expectedMax), (Half)expectedMax);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectRange_BFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedMax = Math.Sqrt(6.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeUniform<BFloat16>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        tensor.Should().BeInRange((BFloat16)(-expectedMax), (BFloat16)expectedMax);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectRange_Float32(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedMax = Math.Sqrt(6.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeUniform<float>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        tensor.Should().BeInRange((float)-expectedMax, (float)expectedMax);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectRange_Float64(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedMax = Math.Sqrt(6.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeUniform<double>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        tensor.Should().BeInRange(-expectedMax, expectedMax);
    }
}
