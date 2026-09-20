// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Arithmetic;

public class NegateShould : IntegrationTestBase
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenLargeFp32(IDevice device)
    {
        // Arrange
        var tensor = Tensor.Random.Uniform(new Shape(1000, 200), -1000f, 1000f, seed: 123456).ToTensor();
        var expected = Tensor.FromArray<float>(tensor.Memory.ToArray().Select(static x => -x).ToArray()).Reshape(tensor.Shape);

        tensor.To(device);

        // Act
        var result = tensor.Negate();

        // Assert
        result.Should().HaveEquivalentElements(expected.ToArray());
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenLargeFp64(IDevice device)
    {
        // Arrange
        var tensor = Tensor.Random.Uniform<double>(new Shape(1000, 200), -1000f, 1000f, seed: 123456).ToTensor();
        var expected = Tensor.FromArray<double>(tensor.Memory.ToArray().Select(static x => -x).ToArray()).Reshape(tensor.Shape);

        tensor.To(device);

        // Act
        var result = tensor.Negate();

        // Assert
        result.Should().HaveEquivalentElements(expected.ToArray());
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenScalar(IDevice device)
    {
        NegateScalarTest<BFloat16>(1, device).Should().Be(-1);
        NegateScalarTest<float>(1, device).Should().Be(-1);
        NegateScalarTest<double>(1, device).Should().Be(-1);
        NegateScalarTest<sbyte>(1, device).Should().Be(-1);
        NegateScalarTest<byte>(1, device).Should().Be(1);
        NegateScalarTest<short>(1, device).Should().Be(-1);
        NegateScalarTest<ushort>(1, device).Should().Be(1);
        NegateScalarTest(1, device).Should().Be(-1);
        NegateScalarTest<uint>(1, device).Should().Be(1);
        NegateScalarTest<long>(1, device).Should().Be(-1);
        NegateScalarTest<ulong>(1, device).Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenVector(IDevice device)
    {
        NegateVectorTest<BFloat16>([1, 2, 3], device).Should().BeEquivalentTo(new BFloat16[] { -1, -2, -3 });
        NegateVectorTest<float>([1, 2, 3], device).Should().BeEquivalentTo(new float[] { -1, -2, -3 });
        NegateVectorTest<double>([1, 2, 3], device).Should().BeEquivalentTo(new double[] { -1, -2, -3 });
        NegateVectorTest<sbyte>([1, 2, 3], device).Should().BeEquivalentTo(new sbyte[] { -1, -2, -3 });
        NegateVectorTest<byte>([1, 2, 3], device).Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        NegateVectorTest<short>([1, 2, 3], device).Should().BeEquivalentTo(new short[] { -1, -2, -3 });
        NegateVectorTest<ushort>([1, 2, 3], device).Should().BeEquivalentTo(new ushort[] { 1, 2, 3 });
        NegateVectorTest<int>([1, 2, 3], device).Should().BeEquivalentTo(new int[] { -1, -2, -3 });
        NegateVectorTest<uint>([1, 2, 3], device).Should().BeEquivalentTo(new uint[] { 1, 2, 3 });
        NegateVectorTest<long>([1, 2, 3], device).Should().BeEquivalentTo(new long[] { -1, -2, -3 });
        NegateVectorTest<ulong>([1, 2, 3], device).Should().BeEquivalentTo(new ulong[] { 1, 2, 3 });
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenMatrix(IDevice device)
    {
        NegateMatrixTest(new BFloat16[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new BFloat16[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new float[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new double[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new sbyte[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new sbyte[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new byte[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new byte[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        NegateMatrixTest(new short[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new short[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new ushort[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new ushort[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        NegateMatrixTest(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new int[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new uint[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new uint[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        NegateMatrixTest(new long[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new long[,] { { -1, -2, -3 }, { -4, -5, -6 } });
        NegateMatrixTest(new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } }, device).Should().BeEquivalentTo(new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } });
    }

    private static Array NegateMatrixTest<TNumber>(TNumber[,] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var matrix = Tensor.FromArray<TNumber>(numbers).ToMatrix();
        matrix.To(device);

        var result = matrix.Negate();

        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void NegateTensor_GivenTensor(IDevice device)
    {
        NegateTensorTest(new BFloat16[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new float[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new float[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new double[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new double[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new sbyte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new byte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        NegateTensorTest(new short[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new short[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new ushort[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        NegateTensorTest(new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new int[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new uint[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
        NegateTensorTest(new long[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new long[,,] { { { -1, -2, -3 }, { -4, -5, -6 } }, { { -7, -8, -9 }, { -10, -11, -12 } } });
        NegateTensorTest(new ulong[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } } });
    }

    private static TNumber NegateScalarTest<TNumber>(TNumber number, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var scalar = new Scalar<TNumber>(number);
        scalar.To(device);

        var result = scalar.Negate();

        return result.Value;
    }

    private static Array NegateVectorTest<TNumber>(TNumber[] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var vector = Tensor.FromArray<TNumber>(numbers).ToVector();
        vector.To(device);

        var result = vector.Negate();

        return result.ToArray();
    }

    private static Array NegateTensorTest<TNumber>(TNumber[,,] numbers, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var tensor = Tensor.FromArray<TNumber>(numbers).ToTensor();
        tensor.To(device);

        var result = tensor.Negate();

        return result.ToArray();
    }
}
