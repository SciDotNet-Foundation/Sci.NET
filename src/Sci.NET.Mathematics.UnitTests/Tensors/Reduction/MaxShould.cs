// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Reduction;

public class MaxShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void MaxAllElements_GivenFloatMatrixAndNoAxis(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } }, requiresGradient: true);
        var expectedGrad = new float[,] { { 0, 0 }, { 0, 0 }, { 0, 1 } };
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Max();

        result.Backward();

        result.To<CpuComputeDevice>();

        // Assert
        result.IsScalar().Should().BeTrue();
        result.ToScalar().Value.Should().Be(6);

        tensor.Gradient?.Should().NotBeNull();
        tensor.Gradient?.Should().HaveEquivalentElements(expectedGrad);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void MaxAllElements_GivenFloatMatrixAndAxis0(IDevice computeDevice)
    {
        // Arrange
        using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } }, requiresGradient: true);
        var expectedGrad = new float[,] { { 0, 0 }, { 0, 0 }, { 1, 1 } };
        tensor.To(computeDevice);

        // Act
        using var result = tensor.Max([0]);

        result.Backward();

        result.To<CpuComputeDevice>();

        // Assert
        result.IsVector().Should().BeTrue();
        result.ToVector().ToArray().Should().BeEquivalentTo(new float[] { 5, 6 });

        tensor.Gradient?.Should().NotBeNull();
        tensor.Gradient?.Should().HaveEquivalentElements(expectedGrad);
    }
}
