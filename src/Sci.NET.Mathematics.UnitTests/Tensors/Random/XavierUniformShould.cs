// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Random;

public class XavierUniformShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMeanAndStdDev_Float16(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        const int fanOut = 200;
        var expectedMax = Math.Sqrt(6.0 / (fanIn + fanOut));

        // Act
        using var tensor = Tensor.Random.XavierUniform<Half>(shape, fanIn, fanOut, device: device);

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
        const int fanOut = 200;
        var expectedMax = Math.Sqrt(6.0 / (fanIn + fanOut));

        // Act
        using var tensor = Tensor.Random.XavierUniform<BFloat16>(shape, fanIn, fanOut, device: device);

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
        const int fanOut = 200;
        var expectedMax = Math.Sqrt(6.0 / (fanIn + fanOut));

        // Act
        using var tensor = Tensor.Random.XavierUniform<float>(shape, fanIn, fanOut, device: device);

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
        const int fanOut = 200;
        var expectedMax = Math.Sqrt(6.0 / (fanIn + fanOut));

        // Act
        using var tensor = Tensor.Random.XavierUniform<double>(shape, fanIn, fanOut, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        tensor.Should().BeInRange(-expectedMax, expectedMax);
    }
}