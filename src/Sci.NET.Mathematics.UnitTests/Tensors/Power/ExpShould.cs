// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Power;

public class ExpShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnCorrectResult_GivenFp32Scalar(IDevice device)
    {
        // Arrange
        using var tensor1 = new Scalar<float>(1.0f);
        using var tensor2 = new Scalar<float>(2.0f);
        using var tensor3 = new Scalar<float>(-1.0f);

        var expected1 = MathF.Exp(1.0f);
        var expected2 = MathF.Exp(2.0f);
        var expected3 = MathF.Exp(-1.0f);

        tensor1.To(device);
        tensor2.To(device);
        tensor3.To(device);

        // Act
        using var result1 = tensor1.Exp();
        using var result2 = tensor2.Exp();
        using var result3 = tensor3.Exp();

        // Assert
        result1.Value.Should().BeApproximately(expected1, 1e-6f);
        result2.Value.Should().BeApproximately(expected2, 1e-6f);
        result3.Value.Should().BeApproximately(expected3, 1e-6f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnCorrectResult_GivenFp64Scalar(IDevice device)
    {
        // Arrange
        using var tensor1 = new Scalar<float>(1.0f);
        using var tensor2 = new Scalar<float>(2.0f);
        using var tensor3 = new Scalar<float>(-1.0f);

        var expected1 = MathF.Exp(1.0f);
        var expected2 = MathF.Exp(2.0f);
        var expected3 = MathF.Exp(-1.0f);

        tensor1.To(device);
        tensor2.To(device);
        tensor3.To(device);

        // Act
        using var result1 = tensor1.Exp();
        using var result2 = tensor2.Exp();
        using var result3 = tensor3.Exp();

        // Assert
        result1.Value.Should().BeApproximately(expected1, 1e-6f);
        result2.Value.Should().BeApproximately(expected2, 1e-6f);
        result3.Value.Should().BeApproximately(expected3, 1e-6f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResultAndGradient_GivenSmallTensorFp32(IDevice device)
    {
        // Arrange
        using var tensor = Tensor.Random.Uniform(new Shape(10), -2.0f, 5.0f, 12345).WithGradient();
        var expectedResult = tensor.Memory.ToArray().Select(MathF.Exp).ToArray();
        var expectedGradient = tensor.Memory.ToArray().Select(MathF.Exp).ToArray();

        tensor.To(device);

        // Act
        using var result = tensor.Exp();
        result.Backward();

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), 1e-6f);
        tensor.Gradient!.Should().NotBeNull();
        tensor.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedGradient.ToArray(), 1e-6f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResultAndGradient_GivenSmallTensorFp64(IDevice device)
    {
        // Arrange
        using var tensor = Tensor.Random.Uniform(new Shape(10), -2.0, 5.0, 12345).WithGradient();
        var expectedResult = tensor.Memory.ToArray().Select(Math.Exp).ToArray();
        var expectedGradient = tensor.Memory.ToArray().Select(Math.Exp).ToArray();

        tensor.To(device);

        // Act
        using var result = tensor.Exp();
        result.Backward();

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), 1e-10f);
        tensor.Gradient!.Should().NotBeNull();
        tensor.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedGradient.ToArray(), 1e-10f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResultAndGradient_GivenLargeTensorFp32(IDevice device)
    {
        // Arrange
        using var tensor = Tensor.Random.Uniform(new Shape(5000), -2.0f, 5.0f, 12345).WithGradient();
        var expectedResult = tensor.Memory.ToArray().Select(MathF.Exp).ToArray();
        var expectedGradient = tensor.Memory.ToArray().Select(MathF.Exp).ToArray();

        tensor.To(device);

        // Act
        using var result = tensor.Exp();
        result.Backward();

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), 1e-4f);
        tensor.Gradient!.Should().NotBeNull();
        tensor.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedGradient.ToArray(), 1e-4f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResultAndGradient_GivenLargeTensorFp64(IDevice device)
    {
        // Arrange
        using var tensor = Tensor.Random.Uniform(new Shape(5000), -2.0, 5.0, 12345).WithGradient();
        var expectedResult = tensor.Memory.ToArray().Select(Math.Exp).ToArray();
        var expectedGradient = tensor.Memory.ToArray().Select(Math.Exp).ToArray();

        tensor.To(device);

        // Act
        using var result = tensor.Exp();
        result.Backward();

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), 1e-10f);
        tensor.Gradient!.Should().NotBeNull();
        tensor.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedGradient.ToArray(), 1e-10f);
    }
}
