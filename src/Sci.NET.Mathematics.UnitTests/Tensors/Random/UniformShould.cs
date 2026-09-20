// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Random;

public class UniformShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenBFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        BFloat16 min = 0.0f;
        BFloat16 max = 1.0f;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        var min = (Half)0.0f;
        var max = (Half)1.0f;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenFloat32(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const float min = 0.0f;
        const float max = 1.0f;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenFloat64(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const double min = 0.0;
        const double max = 1.0;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenInt8(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const sbyte min = -50;
        const sbyte max = 60;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenUInt8(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const byte min = 5;
        const byte max = 200;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenInt16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const short min = -500;
        const short max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenUInt16(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const ushort min = 0;
        const ushort max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenInt32(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const int min = -500;
        const int max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenUInt32(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const int min = 0;
        const int max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenInt64(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const long min = -500;
        const long max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void BeInCorrectRange_GivenUInt64(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 1000);
        const ulong min = 0;
        const ulong max = 500;

        // Act
        using var tensor = Tensor.Random.Uniform(shape, min, max, device: device);

        // Assert
        tensor.Should().HaveShape(shape);

        foreach (var item in tensor.Memory.ToSystemMemory())
        {
            item.Should().BeInRange(min, max);
        }
    }
}
