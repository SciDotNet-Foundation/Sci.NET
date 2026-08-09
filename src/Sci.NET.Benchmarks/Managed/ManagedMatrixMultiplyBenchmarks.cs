// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Backends;
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed;

public class ManagedMatrixMultiplyBenchmarks<TNumber> : IDisposable
    where TNumber : unmanaged, INumber<TNumber>
{
    [ParamsSource(nameof(RowsCols))]
    public ((int Rows, int Columns) Left, (int Rows, int Columns) Right) SizeParam { get; set; }

    public ICollection<((int Rows, int Columns) Left, (int Rows, int Columns) Right)> RowsCols =>
    [
        ((1024, 1024), (1024, 1024)),
        ((1080, 1920), (1080, 1920)),
        ((2048, 2048), (2048, 2048)),
        ((4096, 4096), (4096, 4096)),
        ((8192, 8192), (8192, 8192)),
    ];

    private ILinearAlgebraKernels _linearAlgebraKernels = default!;
    private Matrix<TNumber> _leftMatrix = default!;
    private Matrix<TNumber> _rightMatrix = default!;
    private Matrix<TNumber> _result = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        TNumber min;
        TNumber max;

        Tensor.SetDefaultBackend<ManagedTensorBackend>();

        if (GenericMath.IsFloatingPoint<TNumber>())
        {
            min = TNumber.CreateChecked(-1f);
            max = TNumber.CreateChecked(1f);
        }
        else if (GenericMath.IsSigned<TNumber>())
        {
            min = TNumber.CreateChecked(-10);
            max = TNumber.CreateChecked(10);
        }
        else
        {
            min = TNumber.CreateChecked(1);
            max = TNumber.CreateChecked(10);
        }

        _linearAlgebraKernels = ManagedTensorBackend.Instance.LinearAlgebra;
        _leftMatrix = Tensor.Random.Uniform(new Shape(SizeParam.Left.Rows, SizeParam.Left.Columns), min, max, seed: 123456).ToMatrix();
        _rightMatrix = Tensor.Random.Uniform(new Shape(SizeParam.Right.Columns, SizeParam.Right.Rows), min, max, seed: 654321).ToMatrix();
        _result = Tensor.Zeros<TNumber>(new Shape(SizeParam.Left.Rows, SizeParam.Right.Columns)).ToMatrix();
    }

    [Benchmark]
    public void MatrixMultiply()
    {
        _linearAlgebraKernels.MatrixMultiply(_leftMatrix, _rightMatrix, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _leftMatrix.Dispose();
        _rightMatrix.Dispose();
        _result.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _leftMatrix?.Dispose();
            _rightMatrix?.Dispose();
            _result?.Dispose();
        }
    }
}