// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Reduction;

public class SumShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenFloatMatrixAndNoAxis(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } }, requiresGradient: true);
        var expectedGrad = new float[,] { { 1, 1 }, { 1, 1 }, { 1, 1 } };
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum();

        tensor.Backward();

        result.To<CpuComputeDevice>();

        // Assert
        result.IsScalar().Should().BeTrue();
        result.ToScalar().Value.Should().Be(21);
        tensor.Gradient?.Should().NotBeNull();
        tensor.Gradient?.Should().HaveEquivalentElements(expectedGrad);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenFloatMatrixAndAxis0(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } });
        var expectedGrad = new float[,] { { 1, 0 }, { 1, 0 }, { 1, 0 } };
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum([0]);

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result.ToVector().ToArray().Should().BeEquivalentTo(new float[] { 9, 12 });
        tensor.Gradient?.Should().NotBeNull();
        tensor.Gradient?.Should().HaveEquivalentElements(expectedGrad);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenFloatMatrixAndAxis1(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } });
        var expectedGrad = new float[,] { { 1, 1 }, { 0, 0 }, { 0, 0 } };
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum([1]);

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result.ToVector().ToArray().Should().BeEquivalentTo(new float[] { 3, 7, 11 });
        tensor.Gradient?.Should().NotBeNull();
        tensor.Gradient?.Should().HaveEquivalentElements(expectedGrad);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenFloatMatrixAndAxis0And1(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(Enumerable.Range(0, 60).Select(static x => (float)x).ToArray()).Reshape(3, 4, 5);
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum([0, 1]);

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result.ToVector().ToArray().Should().BeEquivalentTo(new float[] { 330, 342, 354, 366, 378 });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenFloatMatrixAndAxis0And2(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(Enumerable.Range(0, 60).Select(static x => (float)x).ToArray()).Reshape(3, 4, 5);
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum([0, 2]);

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result.ToVector().ToArray().Should().BeEquivalentTo(new float[] { 330, 405, 480, 555 });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenLargeIntMatrix(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<int>(Enumerable.Range(0, 60000).ToArray()).Reshape(30, 40, 50);
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum();

        result.To<CpuComputeDevice>();

        // Assert
        result.IsScalar().Should().BeTrue();
        result
            .ToScalar()
            .Value
            .Should()
            .Be(1799970000);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void SumAllElements_GivenLargeIntMatrixAndAxes0And1(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<int>(Enumerable.Range(0, 60000).ToArray()).Reshape(30, 40, 50);
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Sum([0, 1]);

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result
            .ToVector()
            .ToArray()
            .Should()
            .BeEquivalentTo(
                new int[]
                {
                    35970000, 35971200, 35972400, 35973600, 35974800, 35976000,
                    35977200, 35978400, 35979600, 35980800, 35982000, 35983200,
                    35984400, 35985600, 35986800, 35988000, 35989200, 35990400,
                    35991600, 35992800, 35994000, 35995200, 35996400, 35997600,
                    35998800, 36000000, 36001200, 36002400, 36003600, 36004800,
                    36006000, 36007200, 36008400, 36009600, 36010800, 36012000,
                    36013200, 36014400, 36015600, 36016800, 36018000, 36019200,
                    36020400, 36021600, 36022800, 36024000, 36025200, 36026400,
                    36027600, 36028800
                });
    }
}