// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedHypotKernelBenchmarks<TNumber> : BaseManagedBenchmark
    where TNumber : unmanaged, INumber<TNumber>, IRootFunctions<TNumber>
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
        var min = TNumber.CreateChecked(-1f);
        var max = TNumber.CreateChecked(1f);

        _leftTensor = Tensor.Random.Uniform(Shape, min, max, seed: 123456).ToTensor();
        _rightTensor = Tensor.Random.Uniform(Shape, min, max, seed: 654321).ToTensor();
        _result = Tensor.Zeros<TNumber>(_leftTensor.Shape).ToTensor();
    }

    [Benchmark]
    public void Hypot()
    {
        TensorBackend.LinearAlgebra.Hypot(_leftTensor, _rightTensor, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _leftTensor.Dispose();
        _rightTensor.Dispose();
        _result.Dispose();
    }
}