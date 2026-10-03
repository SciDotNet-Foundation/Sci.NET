// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Linq;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Random;

public class HeNormalShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMeanAndStdDev_Float16(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedStdDev = Math.Sqrt(2.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeNormal<Half>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var (mean, stdDev) = CalculateMeanAndStdDev(tensor.Memory.ToArray());

        mean.Should().BeApproximately(0.0f, 0.01f);
        stdDev.Should().BeApproximately(expectedStdDev, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMeanAndStdDev_BFloat16(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedStdDev = Math.Sqrt(2.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeNormal<BFloat16>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var (mean, stdDev) = CalculateMeanAndStdDev(tensor.Memory.ToArray());

        mean.Should().BeApproximately(0.0f, 0.01f);
        stdDev.Should().BeApproximately(expectedStdDev, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMeanAndStdDev_Float32(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedStdDev = Math.Sqrt(2.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeNormal<float>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var (mean, stdDev) = CalculateMeanAndStdDev(tensor.Memory.ToArray());

        mean.Should().BeApproximately(0.0f, 0.01f);
        stdDev.Should().BeApproximately(expectedStdDev, 0.01f);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void HaveCorrectMeanAndStdDev_Float64(IDevice device)
    {
        // Arrange
        var shape = new Shape(100, 150, 200);
        const int fanIn = 100;
        var expectedStdDev = Math.Sqrt(2.0 / fanIn);

        // Act
        using var tensor = Tensor.Random.HeNormal<double>(shape, fanIn, device: device);

        // Assert
        tensor.Should().HaveShape(shape);
        var (mean, stdDev) = CalculateMeanAndStdDev(tensor.Memory.ToArray());

        mean.Should().BeApproximately(0.0f, 0.01f);
        stdDev.Should().BeApproximately((float)expectedStdDev, 0.01f);
    }

    private static (double Mean, double StdDev) CalculateMeanAndStdDev<TNumber>(TNumber[] data)
        where TNumber : unmanaged, IFloatingPoint<TNumber>, IRootFunctions<TNumber>
    {
        var mean = data.Aggregate(0.0d, static (current, half) => current + double.CreateChecked(half)) / data.Length;
        var variance = data.Select(x => (double.CreateChecked(x) - mean) * (double.CreateChecked(x) - mean)).Mean();
        var stdDev = Math.Sqrt(variance);
        return (mean, stdDev);
    }
}
