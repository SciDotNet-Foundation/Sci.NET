// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedEqualityKernelBenchmarks<TNumber> : BaseManagedBenchmark
    where TNumber : unmanaged, INumber<TNumber>
{
    [ParamsSource(nameof(ShapeOptions))]
    public Shape Shape { get; set; } = default!;

    public ICollection<Shape> ShapeOptions =>
    [
        new Shape(400, 200),
        new Shape(400, 200, 100),
        new Shape(400, 200, 100, 50)
    ];

    private Tensor<TNumber> _leftTensor = default!;
    private Tensor<TNumber> _rightTensor = default!;
    private Tensor<TNumber> _result = default!;

    protected override void SetupBenchmark()
    {
        TNumber min;
        TNumber max;

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

        _leftTensor = Tensor.Random.Uniform(Shape, min, max, seed: 123456).ToTensor();
        _rightTensor = Tensor.Random.Uniform(Shape, min, max, seed: 654321).ToTensor();
        _result = Tensor.Zeros<TNumber>(_leftTensor.Shape).ToTensor();
    }

    [Benchmark]
    public void Equals()
    {
        TensorBackend.EqualityOperations.PointwiseEqual(_leftTensor, _rightTensor, _result);
    }

    [Benchmark]
    public void NotEquals()
    {
        TensorBackend.EqualityOperations.PointwiseNotEqual(_leftTensor, _rightTensor, _result);
    }

    [Benchmark]
    public void GreaterThan()
    {
        TensorBackend.EqualityOperations.PointwiseGreaterThan(_leftTensor, _rightTensor, _result);
    }

    [Benchmark]
    public void GreaterThanOrEquals()
    {
        TensorBackend.EqualityOperations.PointwiseGreaterThanOrEqual(_leftTensor, _rightTensor, _result);
    }

    [Benchmark]
    public void LessThan()
    {
        TensorBackend.EqualityOperations.PointwiseLessThan(_leftTensor, _rightTensor, _result);
    }

    [Benchmark]
    public void LessThanOrEquals()
    {
        TensorBackend.EqualityOperations.PointwiseLessThanOrEqual(_leftTensor, _rightTensor, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _leftTensor.Dispose();
        _rightTensor.Dispose();
        _result.Dispose();
    }
}