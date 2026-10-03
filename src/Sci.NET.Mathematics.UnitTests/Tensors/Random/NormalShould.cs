// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Linq;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Random;

public class NormalShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMean_GivenBFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        BFloat16 mean = 0.0f;
        BFloat16 std = 1.0f;

        // Act
        using var tensor = Tensor.Random.Normal(shape, mean, std, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var array = tensor.Memory.ToArray();

        array.Mean().Should().BeApproximately(0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMean_GivenFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        var mean = (Half)0.0f;
        var std = (Half)1.0f;

        // Act
        using var tensor = Tensor.Random.Normal(shape, mean, std, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var array = tensor.Memory.ToArray();

        array.Mean().Should().BeApproximately((Half)0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMean_GivenFloat32(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const float mean = 0.0f;
        const float std = 1.0f;

        // Act
        using var tensor = Tensor.Random.Normal(shape, mean, std, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var array = tensor.Memory.ToArray();

        array.Mean().Should().BeApproximately(0.0f, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMean_GivenFloat64(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const double mean = 0.0;
        const double std = 1.0;

        // Act
        using var tensor = Tensor.Random.Normal(shape, mean, std, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var array = tensor.Memory.ToArray();

        array.Mean().Should().BeApproximately(0.0f, 0.01f);
    }
}
