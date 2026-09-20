// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.LinearAlgebra;

public class MatrixMultiplyShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenFloatMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new float[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new float[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new float[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenDoubleMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new double[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new double[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new double[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenByteMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new byte[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new byte[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new byte[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenSByteMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new sbyte[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new sbyte[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new sbyte[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenUShortMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new ushort[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new ushort[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new ushort[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenShortMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new short[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new short[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new short[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenUIntMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new uint[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new uint[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new uint[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenIntMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new int[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new int[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new int[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenULongMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new ulong[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new ulong[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new ulong[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLongMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new long[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new long[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new long[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenBFloat16MatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new BFloat16[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new BFloat16[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new BFloat16[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenFloat32MatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new float[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new float[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new float[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenFloat64MatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(
                new double[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 } },
                new double[,] { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } },
                device)
            .Should()
            .BeEquivalentTo(new double[,] { { 30, 30 }, { 30, 30 } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLargerFloat32(IDevice device)
    {
        // Arrange
        const int m = 13;
        const int n = 14;
        const int k = 15;

        using var left = Tensor.FromArray<float>(Enumerable.Range(0, m * n).Select(static x => (float)x).ToArray()).Reshape(m, n).ToMatrix();
        using var right = Tensor.FromArray<float>(Enumerable.Range(0, n * k).Select(static x => (float)x).ToArray()).Reshape(n, k).ToMatrix();

        left.To(device);
        right.To(device);

        // Act
        var result = left.MatrixMultiply(right);

        // Assert
        var expected = new float[m, k];
        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < k; j++)
            {
                float sum = 0;
                for (var p = 0; p < n; p++)
                {
                    sum += left.Memory[(i * n) + p] * right.Memory[(p * k) + j];
                }

                expected[i, j] = sum;
            }
        }

        result.Should().HaveEquivalentElements(expected);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPyTorchExample1(IDevice device)
    {
        MatrixMultiplyTestWithGrad<float>("MatrixMultiply_1", device);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPyTorchExample2(IDevice device)
    {
        MatrixMultiplyTestWithGrad<float>("MatrixMultiply_2", device);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPyTorchExample3(IDevice device)
    {
        MatrixMultiplyTestWithGrad<double>("MatrixMultiply_3", device);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPyTorchExample4(IDevice device)
    {
        MatrixMultiplyTestWithGrad<double>("MatrixMultiply_4", device);
    }

    private static Array MatrixMatrixTest<TNumber>(TNumber[,] left, TNumber[,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftTensor = Tensor.FromArray<TNumber>(left).WithGradient().ToMatrix();
        var rightTensor = Tensor.FromArray<TNumber>(right).WithGradient().ToMatrix();
        leftTensor.To(device);
        rightTensor.To(device);

        var result = leftTensor.MatrixMultiply(rightTensor);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    private static void MatrixMultiplyTestWithGrad<TNumber>(string safetensorsName, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Arrange
        var loadPath = Path.Join(Path.GetDirectoryName(typeof(ContractShould).Assembly.Location) ?? string.Empty, "Tensors", "LinearAlgebra", "Examples", $"{safetensorsName}.safetensors");
        var tensors = Tensor.LoadSafeTensors<TNumber>(loadPath);
        var left = tensors["left"].ToMatrix(requiresGradient: true);
        var right = tensors["right"].ToMatrix(requiresGradient: true);
        var expectedResult = tensors["result"].ToMatrix(requiresGradient: true);
        var expectedLeftGradient = tensors["left_grad"];
        var expectedRightGradient = tensors["right_grad"];
        using var resultGradient = Tensor.Ones<TNumber>(expectedResult.Shape);

        left.To(device);
        right.To(device);
        expectedResult.To(device);
        resultGradient.To(device);
        expectedResult.To(device);
        expectedLeftGradient.To(device);
        expectedRightGradient.To(device);

        // Act
        var result = left.MatrixMultiply(right);
        result.Backward();

        // Assert
        result.Should().HaveApproximatelyEquivalentElements(expectedResult.ToArray(), TNumber.CreateChecked(1e-4f));
        result.Gradient!.Should().NotBeNull();
        result.Gradient!.Should().HaveApproximatelyEquivalentElements(resultGradient.ToArray(), TNumber.CreateChecked(1e-4f));
        left.Gradient!.Should().NotBeNull();
        left.Gradient!.Should().HaveApproximatelyEquivalentElements(expectedLeftGradient.ToArray(), TNumber.CreateChecked(1e-4f));
        right.Gradient!.Should().NotBeNull();
        right.Gradient!.Should().HaveApproximatelyEquivalentElements(expectedRightGradient.ToArray(), TNumber.CreateChecked(1e-4f));
    }
}
