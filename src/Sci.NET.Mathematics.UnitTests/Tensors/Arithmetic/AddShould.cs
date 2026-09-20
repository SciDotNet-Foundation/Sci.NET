// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;
using Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;
using Sci.NET.Mathematics.UnitTests.TestFramework.Integration;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Arithmetic;

public class AddShould : IntegrationTestBase, IArithmeticTests
{
    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenScalarsAndScalar(IDevice device)
    {
        ScalarScalarTest<float>(1, 2, device).Should().Be(3);
        ScalarScalarTest<double>(1, 2, device).Should().Be(3);
        ScalarScalarTest<byte>(1, 2, device).Should().Be(3);
        ScalarScalarTest<sbyte>(1, 2, device).Should().Be(3);
        ScalarScalarTest<ushort>(1, 2, device).Should().Be(3);
        ScalarScalarTest<short>(1, 2, device).Should().Be(3);
        ScalarScalarTest<uint>(1, 2, device).Should().Be(3);
        ScalarScalarTest(1, 2, device).Should().Be(3);
        ScalarScalarTest<ulong>(1, 2, device).Should().Be(3);
        ScalarScalarTest<long>(1, 2, device).Should().Be(3);
        AssertionExtensions.Should(ScalarScalarTest<BFloat16>(1, 2, device)).Be(3);
    }

    private static TNumber ScalarScalarTest<TNumber>(TNumber left, TNumber right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = new Scalar<TNumber>(left);
        var rightScalar = new Scalar<TNumber>(right);
        leftScalar.To(device);
        rightScalar.To(device);

        var resultScalar = leftScalar.Add(rightScalar);

        return resultScalar.Value;
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenScalarAndVector(IDevice device)
    {
        ScalarVectorTest<float>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new float[] { 2, 3, 4, 5 });
        ScalarVectorTest<double>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new double[] { 2, 3, 4, 5 });
        ScalarVectorTest<byte>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new byte[] { 2, 3, 4, 5 });
        ScalarVectorTest<sbyte>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new sbyte[] { 2, 3, 4, 5 });
        ScalarVectorTest<ushort>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new ushort[] { 2, 3, 4, 5 });
        ScalarVectorTest<short>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new short[] { 2, 3, 4, 5 });
        ScalarVectorTest<uint>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new uint[] { 2, 3, 4, 5 });
        ScalarVectorTest<int>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new int[] { 2, 3, 4, 5 });
        ScalarVectorTest<ulong>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new ulong[] { 2, 3, 4, 5 });
        ScalarVectorTest<long>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new long[] { 2, 3, 4, 5 });
        ScalarVectorTest<BFloat16>(1, [1, 2, 3, 4], device).Should().BeEquivalentTo(new BFloat16[] { 2, 3, 4, 5 });
    }

    private static Array ScalarVectorTest<TNumber>(TNumber left, TNumber[] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = new Scalar<TNumber>(left);
        var rightVector = Tensor.FromArray<TNumber>(right).ToVector();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenScalarAndMatrix(IDevice device)
    {
        ScalarMatrixTest(1, new float[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new float[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest(1, new double[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new double[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<byte>(1, new byte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new byte[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<sbyte>(1, new sbyte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new sbyte[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<ushort>(1, new ushort[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ushort[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<short>(1, new short[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new short[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<uint>(1, new uint[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new uint[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest(1, new int[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new int[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest<ulong>(1, new ulong[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ulong[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest(1, new long[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new long[,] { { 2, 3 }, { 4, 5 } });
        ScalarMatrixTest(1, new BFloat16[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new BFloat16[,] { { 2, 3 }, { 4, 5 } });
    }

    private static Array ScalarMatrixTest<TNumber>(TNumber left, TNumber[,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = new Scalar<TNumber>(left);
        var rightVector = Tensor.FromArray<TNumber>(right).ToMatrix();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenScalarTensor(IDevice device)
    {
        ScalarTensorTest(1, new float[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new float[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest(1, new double[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new double[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<byte>(1, new byte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<sbyte>(1, new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<sbyte>(1, new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<byte>(1, new byte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<sbyte>(1, new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<ushort>(1, new ushort[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<short>(1, new short[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new short[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<uint>(1, new uint[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest(1, new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new int[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest<ulong>(1, new ulong[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest(1, new long[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new long[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        ScalarTensorTest(1, new BFloat16[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
    }

    private static Array ScalarTensorTest<TNumber>(TNumber left, TNumber[,,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = new Scalar<TNumber>(left);
        var rightVector = Tensor.FromArray<TNumber>(right).ToTensor();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenVectorAndScalar(IDevice device)
    {
        VectorScalarTest<float>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new float[] { 2, 3, 4, 5 });
        VectorScalarTest<double>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new double[] { 2, 3, 4, 5 });
        VectorScalarTest<byte>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new byte[] { 2, 3, 4, 5 });
        VectorScalarTest<sbyte>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new sbyte[] { 2, 3, 4, 5 });
        VectorScalarTest<ushort>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new ushort[] { 2, 3, 4, 5 });
        VectorScalarTest<short>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new short[] { 2, 3, 4, 5 });
        VectorScalarTest<uint>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new uint[] { 2, 3, 4, 5 });
        VectorScalarTest<int>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new int[] { 2, 3, 4, 5 });
        VectorScalarTest<ulong>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new ulong[] { 2, 3, 4, 5 });
        VectorScalarTest<long>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new long[] { 2, 3, 4, 5 });
        VectorScalarTest<BFloat16>([1, 2, 3, 4], 1, device).Should().BeEquivalentTo(new BFloat16[] { 2, 3, 4, 5 });
    }

    private static Array VectorScalarTest<TNumber>(TNumber[] left, TNumber right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToVector();
        var rightVector = new Scalar<TNumber>(right);
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenVectorAndVector(IDevice device)
    {
        VectorVectorTest<float>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new float[] { 2, 4, 6, 8 });
        VectorVectorTest<double>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new double[] { 2, 4, 6, 8 });
        VectorVectorTest<byte>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new byte[] { 2, 4, 6, 8 });
        VectorVectorTest<sbyte>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new sbyte[] { 2, 4, 6, 8 });
        VectorVectorTest<ushort>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new ushort[] { 2, 4, 6, 8 });
        VectorVectorTest<short>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new short[] { 2, 4, 6, 8 });
        VectorVectorTest<uint>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new uint[] { 2, 4, 6, 8 });
        VectorVectorTest<int>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new int[] { 2, 4, 6, 8 });
        VectorVectorTest<ulong>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new ulong[] { 2, 4, 6, 8 });
        VectorVectorTest<long>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new long[] { 2, 4, 6, 8 });
        VectorVectorTest<BFloat16>([1, 2, 3, 4], [1, 2, 3, 4], device).Should().BeEquivalentTo(new BFloat16[] { 2, 4, 6, 8 });
    }

    private static Array VectorVectorTest<TNumber>(TNumber[] left, TNumber[] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToVector();
        var rightVector = Tensor.FromArray<TNumber>(right).ToVector();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenVectorAndMatrix(IDevice device)
    {
        VectorMatrixTest<float>([1, 2, 3, 4], new float[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new float[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<double>([1, 2, 3, 4], new double[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new double[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<byte>([1, 2, 3, 4], new byte[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new byte[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<sbyte>([1, 2, 3, 4], new sbyte[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new sbyte[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<ushort>([1, 2, 3, 4], new ushort[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new ushort[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<short>([1, 2, 3, 4], new short[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new short[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<uint>([1, 2, 3, 4], new uint[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new uint[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<int>([1, 2, 3, 4], new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new int[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<ulong>([1, 2, 3, 4], new ulong[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new ulong[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<long>([1, 2, 3, 4], new long[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new long[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
        VectorMatrixTest<BFloat16>([1, 2, 3, 4], new BFloat16[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 } }, device).Should().BeEquivalentTo(new BFloat16[,] { { 2, 4, 6, 8 }, { 6, 8, 10, 12 } });
    }

    private static Array VectorMatrixTest<TNumber>(TNumber[] left, TNumber[,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToVector();
        var rightMatrix = Tensor.FromArray<TNumber>(right).ToMatrix();
        leftScalar.To(device);
        rightMatrix.To(device);

        var result = leftScalar.Add(rightMatrix);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenVectorAndTensor(IDevice device)
    {
        VectorTensorTest<float>([1, 2], new float[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new float[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<double>([1, 2], new double[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new double[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<byte>([1, 2], new byte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<sbyte>([1, 2], new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<ushort>([1, 2], new ushort[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<short>([1, 2], new short[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new short[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<uint>([1, 2], new uint[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<int>([1, 2], new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new int[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<ulong>([1, 2], new ulong[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<long>([1, 2], new long[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new long[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
        VectorTensorTest<BFloat16>([1, 2], new BFloat16[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 4 }, { 4, 6 } }, { { 6, 8 }, { 8, 10 } } });
    }

    private static Array VectorTensorTest<TNumber>(TNumber[] left, TNumber[,,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToVector();
        var rightTensor = Tensor.FromArray<TNumber>(right).ToTensor();
        leftScalar.To(device);
        rightTensor.To(device);

        var result = leftScalar.Add(rightTensor);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenMatrixAndScalar(IDevice device)
    {
        MatrixScalarTest(new float[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new float[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest(new double[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new double[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<byte>(new byte[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new byte[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<sbyte>(new sbyte[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new sbyte[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<ushort>(new ushort[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new ushort[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<short>(new short[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new short[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<uint>(new uint[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new uint[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest(new int[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new int[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest<ulong>(new ulong[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new ulong[,] { { 2, 3 }, { 4, 5 } });
        MatrixScalarTest(new BFloat16[,] { { 1, 2 }, { 3, 4 } }, 1, device).Should().BeEquivalentTo(new BFloat16[,] { { 2, 3 }, { 4, 5 } });
    }

    private static Array MatrixScalarTest<TNumber>(TNumber[,] left, TNumber right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToMatrix();
        var rightVector = new Scalar<TNumber>(right);
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenMatrixAndVector(IDevice device)
    {
        MatrixVectorTest<float>(new float[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new float[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<double>(new double[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new double[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<byte>(new byte[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new byte[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<sbyte>(new sbyte[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new sbyte[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<ushort>(new ushort[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new ushort[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<short>(new short[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new short[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<uint>(new uint[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new uint[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<int>(new int[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new int[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<ulong>(new ulong[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new ulong[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<long>(new long[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new long[,] { { 2, 4 }, { 4, 6 } });
        MatrixVectorTest<BFloat16>(new BFloat16[,] { { 1, 2 }, { 3, 4 } }, [1, 2], device).Should().BeEquivalentTo(new BFloat16[,] { { 2, 4 }, { 4, 6 } });
    }

    private static Array MatrixVectorTest<TNumber>(TNumber[,] left, TNumber[] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToMatrix();
        var rightVector = Tensor.FromArray<TNumber>(right).ToVector();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenMatrixAndMatrix(IDevice device)
    {
        MatrixMatrixTest(new float[,] { { 1, 2 }, { 3, 4 } }, new float[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new float[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new double[,] { { 1, 2 }, { 3, 4 } }, new double[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new double[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new byte[,] { { 1, 2 }, { 3, 4 } }, new byte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new byte[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new sbyte[,] { { 1, 2 }, { 3, 4 } }, new sbyte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new sbyte[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new ushort[,] { { 1, 2 }, { 3, 4 } }, new ushort[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ushort[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new short[,] { { 1, 2 }, { 3, 4 } }, new short[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new short[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new uint[,] { { 1, 2 }, { 3, 4 } }, new uint[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new uint[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new int[,] { { 1, 2 }, { 3, 4 } }, new int[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new int[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new ulong[,] { { 1, 2 }, { 3, 4 } }, new ulong[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ulong[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new long[,] { { 1, 2 }, { 3, 4 } }, new long[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new long[,] { { 2, 4 }, { 6, 8 } });
        MatrixMatrixTest(new BFloat16[,] { { 1, 2 }, { 3, 4 } }, new BFloat16[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new BFloat16[,] { { 2, 4 }, { 6, 8 } });
    }

    private static Array MatrixMatrixTest<TNumber>(TNumber[,] left, TNumber[,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToMatrix();
        var rightMatrix = Tensor.FromArray<TNumber>(right).ToMatrix();
        leftScalar.To(device);
        rightMatrix.To(device);

        var result = leftScalar.Add(rightMatrix);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenMatrixAndTensor(IDevice device)
    {
        MatrixTensorTest(new float[,] { { 1, 2 }, { 3, 4 } }, new float[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new float[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new double[,] { { 1, 2 }, { 3, 4 } }, new double[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new double[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new byte[,] { { 1, 2 }, { 3, 4 } }, new byte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new sbyte[,] { { 1, 2 }, { 3, 4 } }, new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new ushort[,] { { 1, 2 }, { 3, 4 } }, new ushort[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new short[,] { { 1, 2 }, { 3, 4 } }, new short[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new short[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new uint[,] { { 1, 2 }, { 3, 4 } }, new uint[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new int[,] { { 1, 2 }, { 3, 4 } }, new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new int[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new ulong[,] { { 1, 2 }, { 3, 4 } }, new ulong[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new long[,] { { 1, 2 }, { 3, 4 } }, new long[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new long[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
        MatrixTensorTest(new BFloat16[,] { { 1, 2 }, { 3, 4 } }, new BFloat16[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 4 }, { 6, 8 } }, { { 6, 8 }, { 10, 12 } } });
    }

    private static Array MatrixTensorTest<TNumber>(TNumber[,] left, TNumber[,,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToMatrix();
        var rightTensor = Tensor.FromArray<TNumber>(right).ToTensor();
        leftScalar.To(device);
        rightTensor.To(device);

        var result = leftScalar.Add(rightTensor);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenTensorAndScalar(IDevice device)
    {
        TensorScalarTest(new float[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new float[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest(new double[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new double[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<byte>(new byte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<sbyte>(new sbyte[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<ushort>(new ushort[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<short>(new short[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new short[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<uint>(new uint[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest(new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new int[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest<ulong>(new ulong[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest(new long[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new long[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
        TensorScalarTest(new BFloat16[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } }, 1, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 3 }, { 4, 5 } }, { { 6, 7 }, { 8, 9 } } });
    }

    private static Array TensorScalarTest<TNumber>(TNumber[,,] left, TNumber right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToTensor();
        var rightVector = new Scalar<TNumber>(right);
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenTensorAndVector(IDevice device)
    {
        TensorVectorTest<float>(new float[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new float[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<double>(new double[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new double[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<byte>(new byte[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new byte[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<sbyte>(new sbyte[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<ushort>(new ushort[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<short>(new short[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new short[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<uint>(new uint[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new uint[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<int>(new int[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new int[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<ulong>(new ulong[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<long>(new long[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new long[,,] { { { 2, 4 }, { 4, 6 } } });
        TensorVectorTest<BFloat16>(new BFloat16[,,] { { { 1, 2 }, { 3, 4 } } }, [1, 2], device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 4 }, { 4, 6 } } });
    }

    private static Array TensorVectorTest<TNumber>(TNumber[,,] left, TNumber[] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToTensor();
        var rightVector = Tensor.FromArray<TNumber>(right).ToVector();
        leftScalar.To(device);
        rightVector.To(device);

        var result = leftScalar.Add(rightVector);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenTensorAndMatrix(IDevice device)
    {
        TensorMatrixTest(new float[,,] { { { 1, 2 }, { 3, 4 } } }, new float[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new float[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new double[,,] { { { 1, 2 }, { 3, 4 } } }, new double[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new double[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new byte[,,] { { { 1, 2 }, { 3, 4 } } }, new byte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new sbyte[,,] { { { 1, 2 }, { 3, 4 } } }, new sbyte[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new ushort[,,] { { { 1, 2 }, { 3, 4 } } }, new ushort[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new short[,,] { { { 1, 2 }, { 3, 4 } } }, new short[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new short[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new uint[,,] { { { 1, 2 }, { 3, 4 } } }, new uint[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new int[,,] { { { 1, 2 }, { 3, 4 } } }, new int[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new int[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new ulong[,,] { { { 1, 2 }, { 3, 4 } } }, new ulong[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new long[,,] { { { 1, 2 }, { 3, 4 } } }, new long[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new long[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorMatrixTest(new BFloat16[,,] { { { 1, 2 }, { 3, 4 } } }, new BFloat16[,] { { 1, 2 }, { 3, 4 } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 4 }, { 6, 8 } } });
    }

    private static Array TensorMatrixTest<TNumber>(TNumber[,,] left, TNumber[,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToTensor();
        var rightMatrix = Tensor.FromArray<TNumber>(right).ToMatrix();
        leftScalar.To(device);
        rightMatrix.To(device);

        var result = leftScalar.Add(rightMatrix);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenTensorAndTensor(IDevice device)
    {
        TensorTensorTest(new double[,,] { { { 1, 2 }, { 3, 4 } } }, new double[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new double[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new float[,,] { { { 1, 2 }, { 3, 4 } } }, new float[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new float[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new byte[,,] { { { 1, 2 }, { 3, 4 } } }, new byte[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new byte[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new sbyte[,,] { { { 1, 2 }, { 3, 4 } } }, new sbyte[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new sbyte[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new ushort[,,] { { { 1, 2 }, { 3, 4 } } }, new ushort[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new ushort[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new short[,,] { { { 1, 2 }, { 3, 4 } } }, new short[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new short[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new uint[,,] { { { 1, 2 }, { 3, 4 } } }, new uint[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new uint[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new int[,,] { { { 1, 2 }, { 3, 4 } } }, new int[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new int[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new ulong[,,] { { { 1, 2 }, { 3, 4 } } }, new ulong[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new ulong[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new long[,,] { { { 1, 2 }, { 3, 4 } } }, new long[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new long[,,] { { { 2, 4 }, { 6, 8 } } });
        TensorTensorTest(new BFloat16[,,] { { { 1, 2 }, { 3, 4 } } }, new BFloat16[,,] { { { 1, 2 }, { 3, 4 } } }, device).Should().BeEquivalentTo(new BFloat16[,,] { { { 2, 4 }, { 6, 8 } } });
    }

    private static Array TensorTensorTest<TNumber>(TNumber[,,] left, TNumber[,,] right, IDevice device)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftScalar = Tensor.FromArray<TNumber>(left).ToTensor();
        var rightTensor = Tensor.FromArray<TNumber>(right).ToTensor();
        leftScalar.To(device);
        rightTensor.To(device);

        var result = leftScalar.Add(rightTensor);

        result.To<CpuComputeDevice>();
        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLargeIntTensors(IDevice device)
    {
        // Arrange
        var a = Tensor
            .FromArray<int>(Enumerable.Range(0, 125000).ToArray())
            .Reshape(50, 50, 50);

        var b = Tensor
            .FromArray<int>(Enumerable.Range(0, 125000).ToArray())
            .Reshape(50, 50, 50);

        a.To(device);
        b.To(device);

        // Act
        var result = a.Add(b);

        // Assert
        var resultArray = result.ToArray();
        var expectedArray = Enumerable.Range(0, 125000).Select(static x => x * 2).ToArray();

        resultArray.Should().BeEquivalentTo(expectedArray);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenLargeLongTensors(IDevice device)
    {
        // Arrange
        var a = Tensor
            .FromArray<long>(Enumerable.Range(0, 125000).Select(static x => (long)x).ToArray())
            .Reshape(50, 50, 50);

        var b = Tensor
            .FromArray<long>(Enumerable.Range(0, 125000).Select(static x => (long)x).ToArray())
            .Reshape(50, 50, 50);

        a.To(device);
        b.To(device);

        // Act
        var result = a.Add(b);

        // Assert
        var resultArray = result.ToArray();
        var expectedArray = Enumerable.Range(0, 125000).Select(static x => (long)(x * 2)).ToArray();

        resultArray.Should().BeEquivalentTo(expectedArray);
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPytorchExample_1(IDevice device)
    {
        TestAddOp<double>("Add_1", device, static (x, y) => x.Add(y));
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPytorchExample_2(IDevice device)
    {
        TestAddOp<double>("Add_2", device, static (x, y) => x.Add(y));
    }

    [Theory]
    [MemberData(nameof(ComputeDevices))]
    public void ReturnExpectedResult_GivenPytorchExample_3(IDevice device)
    {
        TestAddOp<double>("Add_3", device, static (x, y) => x.Add(y));
    }

    private static void TestAddOp<TNumber>(string fileName, IDevice device, Func<ITensor<TNumber>, ITensor<TNumber>, ITensor<TNumber>> function)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var filePath = Path.Combine(Path.GetDirectoryName(typeof(AddShould).Assembly.Location) ?? string.Empty, "Tensors", "Arithmetic", "Examples", $"{fileName}.safetensors");
        var tensors = Tensor.LoadSafeTensors<TNumber>(filePath);
        var left = tensors["left"].ToTensor();
        var right = tensors["right"].ToTensor();
        var expected = tensors["result"].ToTensor();
        var expectedLeftGradient = tensors["left_grad"].ToTensor();
        var expectedRightGradient = tensors["right_grad"].ToTensor();
        using var resultGradient = Tensor.Ones<TNumber>(expected.Shape);

        left.To(device);
        right.To(device);

        var result = function(left, right);
        result.Backward();

        result.To<CpuComputeDevice>();
        left.To<CpuComputeDevice>();
        right.To<CpuComputeDevice>();

        result.Should().HaveApproximatelyEquivalentElements(expected.ToArray(), TNumber.CreateChecked(1e-4f));
        result.Gradient?.Should().NotBeNull();
        result.Gradient?.Should().HaveApproximatelyEquivalentElements(resultGradient.ToArray(), TNumber.CreateChecked(1e-4f));
        left.Gradient?.Should().NotBeNull();
        left.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedLeftGradient.ToArray(), TNumber.CreateChecked(1e-4f));
        right.Gradient?.Should().NotBeNull();
        right.Gradient?.Should().HaveApproximatelyEquivalentElements(expectedRightGradient.ToArray(), TNumber.CreateChecked(1e-4f));
    }
}
