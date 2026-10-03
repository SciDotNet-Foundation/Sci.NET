// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Arithmetic;

public class AbsShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenScalar(IDevice device)
    {
        AbsScalarTest<float>(-1, device).Should().Be(1);
        AbsScalarTest<double>(-1, device).Should().Be(1);
        AbsScalarTest<sbyte>(-1, device).Should().Be(1);
        AbsScalarTest<byte>(1, device).Should().Be(1);
        AbsScalarTest<short>(-1, device).Should().Be(1);
        AbsScalarTest<ushort>(1, device).Should().Be(1);
        AbsScalarTest(-1, device).Should().Be(1);
        AbsScalarTest<uint>(1, device).Should().Be(1);
        AbsScalarTest<long>(-1, device).Should().Be(1);
        AbsScalarTest<ulong>(1, device).Should().Be(1);
    }

    private static TNumber AbsScalarTest<TNumber>(TNumber number, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var scalar = new Scalar<TNumber>(number);

        scalar.To(device);

        var result = scalar.Abs();

        return result.Value;
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenVector(IDevice device)
    {
        AbsVectorTest<float>([-1, -2, -3], device).Should().BeEquivalentTo(new float[] { 1, 2, 3 });
        AbsVectorTest<double>([-1, -2, -3], device).Should().BeEquivalentTo(new double[] { 1, 2, 3 });
        AbsVectorTest<sbyte>([-1, -2, -3], device).Should().BeEquivalentTo(new sbyte[] { 1, 2, 3 });
        AbsVectorTest<byte>([1, 2, 3], device).Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        AbsVectorTest<short>([-1, -2, -3], device).Should().BeEquivalentTo(new short[] { 1, 2, 3 });
        AbsVectorTest<ushort>([1, 2, 3], device).Should().BeEquivalentTo(new ushort[] { 1, 2, 3 });
        AbsVectorTest<int>([-1, -2, -3], device).Should().BeEquivalentTo(new int[] { 1, 2, 3 });
        AbsVectorTest<uint>([1, 2, 3], device).Should().BeEquivalentTo(new uint[] { 1, 2, 3 });
        AbsVectorTest<long>([-1, -2, -3], device).Should().BeEquivalentTo(new long[] { 1, 2, 3 });
        AbsVectorTest<ulong>([1, 2, 3], device).Should().BeEquivalentTo(new ulong[] { 1, 2, 3 });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenMatrix(IDevice device)
    {
        AbsMatrixTest(new float[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new double[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new sbyte[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new sbyte[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new byte[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new byte[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new short[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new short[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new ushort[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new ushort[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new int[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new uint[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new uint[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new long[,] { { -1, -2, -3 }, { -4, -5, -6 } }, device).Should().BeEquivalentTo(new long[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        AbsMatrixTest(new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } });
    }

    private static Array AbsMatrixTest<TNumber>(TNumber[,] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var matrix = Tensor.FromArray<TNumber>(numbers).ToMatrix();

        matrix.To(device);

        var result = matrix.Abs();

        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenTensor(IDevice device)
    {
        AbsTensorTest(new float[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } }, device).Should().BeEquivalentTo(new float[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        AbsTensorTest(new double[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } }, device).Should().BeEquivalentTo(new double[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        AbsTensorTest(new sbyte[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        AbsTensorTest(new byte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        AbsTensorTest(new short[,,] { { { -1, -2, -3, -4 }, { -5, -6, -7, -8 } }, { { -9, -10, -11, -12 }, { -13, -14, -15, -16 } } }, device).Should().BeEquivalentTo(new short[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
        AbsTensorTest(new ushort[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
        AbsTensorTest(new int[,,] { { { -1, -2, -3, -4 }, { -5, -6, -7, -8 } }, { { -9, -10, -11, -12 }, { -13, -14, -15, -16 } } }, device).Should().BeEquivalentTo(new int[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
        AbsTensorTest(new uint[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
        AbsTensorTest(new long[,,] { { { -1, -2, -3, -4 }, { -5, -6, -7, -8 } }, { { -9, -10, -11, -12 }, { -13, -14, -15, -16 } } }, device).Should().BeEquivalentTo(new long[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
        AbsTensorTest(new ulong[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, { { 9, 10, 11, 12 }, { 13, 14, 15, 16 } } });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLargeTensorFp32(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 200);
        var randomTensor = Tensor.Random.Uniform(shape, -100f, 100f, seed: 123456).ToTensor();
        var expected = Tensor.FromArray<float>(randomTensor.Memory.ToArray().Select(MathF.Abs).ToArray()).Reshape(shape);

        randomTensor.To(device);

        // Act
        var result = randomTensor.Abs();

        // Assert
        result.Should().HaveEquivalentElements(expected.ToArray());
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLargeTensorFp64(IDevice device)
    {
        // Arrange
        var shape = new Shape(1000, 200);
        var randomTensor = Tensor.Random.Uniform<double>(shape, -100f, 100f, seed: 123456).ToTensor();
        var expected = Tensor.FromArray<double>(randomTensor.Memory.ToArray().Select(Math.Abs).ToArray()).Reshape(shape);

        randomTensor.To(device);

        // Act
        var result = randomTensor.Abs();

        // Assert
        result.Should().HaveEquivalentElements(expected.ToArray());
    }

    private static Array AbsVectorTest<TNumber>(TNumber[] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var vector = Tensor.FromArray<TNumber>(numbers).ToVector();

        vector.To(device);

        var result = vector.Abs();

        return result.ToArray();
    }

    private static Array AbsTensorTest<TNumber>(TNumber[,,] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var tensor = Tensor.FromArray<TNumber>(numbers).ToTensor();

        tensor.To(device);

        var result = tensor.Abs();

        return result.ToArray();
    }
}
