// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.NeuralNetworks.ActivationFunctions;

public class HardSigmoidShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnCorrectValues_GivenFloat(IDevice device)
    {
        // Arrange
        var value = Tensor.FromArray<float>(new float[] { -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 });

        value.To(device);

        // Act
        var result = value.HardSigmoid();

        // Assert
        result
            .Should()
            .HaveShape(10)
            .And
            .HaveEquivalentElements(new float[] { 0F, 0F, 0.16666666F, 0.3333333F, 0.5F, 0.6666667F, 0.8333334F, 1F, 1F, 1F });
    }
}