// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.LinearAlgebra;

public class ContractShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnsCorrectResult_GivenTwoTensorsWithSameShape(IDevice device)
    {
        // Arrange
        var left = Tensor.FromArray<float>(Enumerable.Range(0, 4 * 3 * 2).Select(static x => (float)x).ToArray()).Reshape(4, 3, 2);
        var right = Tensor.FromArray<float>(Enumerable.Range(0, 4 * 3 * 2).Select(static x => (float)x).ToArray()).Reshape(4, 3, 2);

        left.To(device);
        right.To(device);

        var result = left.Contract(right, [1, 2], [1, 2]);

        result
            .Should()
            .HaveShape(4, 4)
            .And
            .HaveEquivalentElements(new float[,] { { 55, 145, 235, 325 }, { 145, 451, 757, 1063 }, { 235, 757, 1279, 1801 }, { 325, 1063, 1801, 2539 } });

        left.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } }
        });

        right.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } },
            { { 36.0f, 40.0f }, { 44.0f, 48.0f }, { 52.0f, 56.0f } }
        });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnsCorrectResult_Example1(IDevice device)
    {
        // Arrange
        var left = Tensor.FromArray<int>(Enumerable.Range(0, 2 * 3 * 4).ToArray()).Reshape(2, 3, 4);
        var right = Tensor.FromArray<int>(Enumerable.Range(0, 3 * 4 * 5).ToArray()).Reshape(3, 4, 5);

        var expected = new int[,,,]
        {
            {
                { { 400, 412, 424, 436, 448 }, { 460, 472, 484, 496, 508 }, { 520, 532, 544, 556, 568 }, { 580, 592, 604, 616, 628 } },
                { { 460, 475, 490, 505, 520 }, { 535, 550, 565, 580, 595 }, { 610, 625, 640, 655, 670 }, { 685, 700, 715, 730, 745 } },
                { { 520, 538, 556, 574, 592 }, { 610, 628, 646, 664, 682 }, { 700, 718, 736, 754, 772 }, { 790, 808, 826, 844, 862 } },
                { { 580, 601, 622, 643, 664 }, { 685, 706, 727, 748, 769 }, { 790, 811, 832, 853, 874 }, { 895, 916, 937, 958, 979 } }
            },
            {
                { { 1120, 1168, 1216, 1264, 1312 }, { 1360, 1408, 1456, 1504, 1552 }, { 1600, 1648, 1696, 1744, 1792 }, { 1840, 1888, 1936, 1984, 2032 } },
                { { 1180, 1231, 1282, 1333, 1384 }, { 1435, 1486, 1537, 1588, 1639 }, { 1690, 1741, 1792, 1843, 1894 }, { 1945, 1996, 2047, 2098, 2149 } },
                { { 1240, 1294, 1348, 1402, 1456 }, { 1510, 1564, 1618, 1672, 1726 }, { 1780, 1834, 1888, 1942, 1996 }, { 2050, 2104, 2158, 2212, 2266 } },
                { { 1300, 1357, 1414, 1471, 1528 }, { 1585, 1642, 1699, 1756, 1813 }, { 1870, 1927, 1984, 2041, 2098 }, { 2155, 2212, 2269, 2326, 2383 } }
            }
        };

        left.To(device);
        right.To(device);

        // Act
        var result = left.Contract(right, [1], [0]);
        result.Backward();

        // Assert
        result.Should().HaveEquivalentElements(expected);

        left.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            { { 190.0f, 190.0f, 190.0f, 190.0f }, { 590.0f, 590.0f, 590.0f, 590.0f }, { 990.0f, 990.0f, 990.0f, 990.0f } },
            { { 190.0f, 190.0f, 190.0f, 190.0f }, { 590.0f, 590.0f, 590.0f, 590.0f }, { 990.0f, 990.0f, 990.0f, 990.0f } }
        });

        right.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            {
                { 60.0f, 60.0f, 60.0f, 60.0f, 60.0f },
                { 60.0f, 60.0f, 60.0f, 60.0f, 60.0f },
                { 60.0f, 60.0f, 60.0f, 60.0f, 60.0f },
                { 60.0f, 60.0f, 60.0f, 60.0f, 60.0f }
            },
            {
                { 92.0f, 92.0f, 92.0f, 92.0f, 92.0f },
                { 92.0f, 92.0f, 92.0f, 92.0f, 92.0f },
                { 92.0f, 92.0f, 92.0f, 92.0f, 92.0f },
                { 92.0f, 92.0f, 92.0f, 92.0f, 92.0f }
            },
            {
                { 124.0f, 124.0f, 124.0f, 124.0f, 124.0f },
                { 124.0f, 124.0f, 124.0f, 124.0f, 124.0f },
                { 124.0f, 124.0f, 124.0f, 124.0f, 124.0f },
                { 124.0f, 124.0f, 124.0f, 124.0f, 124.0f }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnsCorrectResult_Example2(IDevice device)
    {
        // Arrange
        var left = Tensor.FromArray<int>(Enumerable.Range(0, 2 * 5 * 4).ToArray()).Reshape(2, 5, 4);
        var right = Tensor.FromArray<int>(Enumerable.Range(0, 2 * 5 * 2).ToArray()).Reshape(2, 5, 2);

        var expected = new int[,] { { 2280, 2460 }, { 2370, 2560 }, { 2460, 2660 }, { 2550, 2760 } };

        left.To(device);
        right.To(device);

        // Act
        var result = left.Contract(right, [0, 1], [0, 1]);

        // Assert
        result.Should().HaveEquivalentElements(expected);

        left.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            {
                { 1.0f, 1.0f, 1.0f, 1.0f },
                { 5.0f, 5.0f, 5.0f, 5.0f },
                { 9.0f, 9.0f, 9.0f, 9.0f },
                { 13.0f, 13.0f, 13.0f, 13.0f },
                { 17.0f, 17.0f, 17.0f, 17.0f }
            },
            {
                { 21.0f, 21.0f, 21.0f, 21.0f },
                { 25.0f, 25.0f, 25.0f, 25.0f },
                { 29.0f, 29.0f, 29.0f, 29.0f },
                { 33.0f, 33.0f, 33.0f, 33.0f },
                { 37.0f, 37.0f, 37.0f, 37.0f }
            }
        });

        right.Gradient?.Should().HaveEquivalentElements(new float[,,]
        {
            {
                { 6.0f, 6.0f },
                { 22.0f, 22.0f },
                { 38.0f, 38.0f },
                { 54.0f, 54.0f },
                { 70.0f, 70.0f }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnsCorrectResult_GivenPyTorchExample1(IDevice device)
    {
        PyTorchTest<long>("Contract_[[1]_[0]]_1", device, [1], [0]);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnsCorrectResult_GivenPyTorchExample2(IDevice device)
    {
        PyTorchTest<int>("Contract_[[1]_[0]]_2", device, [1], [0]);
    }

    private static void PyTorchTest<TNumber>(string safetensorsName, IDevice device, int[] leftIndices, int[] rightIndices)
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Arrange
        var loadPath = Path.Join(Path.GetDirectoryName(typeof(ContractShould).Assembly.Location) ?? string.Empty, "Tensors", "LinearAlgebra", "Examples", $"{safetensorsName}.safetensors");
        var tensors = Tensor.LoadSafeTensors<TNumber>(loadPath);
        var left = tensors["left"];
        var right = tensors["right"];
        var expectedResult = tensors["result"];

        left.To(device);
        right.To(device);
        expectedResult.To(device);

        // Act
        var result = left.Contract(right, leftIndices, rightIndices);

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), TNumber.CreateChecked(1e-7f));
    }
}
