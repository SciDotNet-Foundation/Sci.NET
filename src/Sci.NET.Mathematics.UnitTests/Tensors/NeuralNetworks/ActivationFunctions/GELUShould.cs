// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.NeuralNetworks.ActivationFunctions;

// ReSharper disable once InconsistentNaming
public class GELUShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnCorrectValues_GivenFloat(IDevice device)
    {
        // Arrange
        var value = Tensor.FromArray<float>(new float[] { -4, -2, -1, -0.75f, 0, 1, 2, 50, 60 });

        value.To(device);

        // Act
        var result = value.GELU();

        // Assert
        result
            .Should()
            .HaveShape(9)
            .And
            .HaveApproximatelyEquivalentElements(new float[] { -7.021427E-05f, -0.04540229f, -0.15880802f, -0.17003945f, 0, 0.841192f, 1.9545977f, 50, 60 }, 1e-6f);
    }
}
