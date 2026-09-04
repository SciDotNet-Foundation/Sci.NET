// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedActivationFunctionKernelBenchmarks<TNumber> : BaseManagedBenchmark
    where TNumber : unmanaged, IFloatingPointIeee754<TNumber>
{
    [ParamsSource(nameof(ShapeOptions))]
    public Shape Shape { get; set; } = default!;

    public ICollection<Shape> ShapeOptions =>
    [
        new Shape(400, 200),
        new Shape(400, 200, 100),
        new Shape(400, 200, 100, 50),
    ];

    private Tensor<TNumber> _tensor = default!;
    private ITensor<TNumber> _result = default!;
    private TNumber _alpha;
    private TNumber _min;
    private TNumber _max;

    protected override void SetupBenchmark()
    {
        var min = TNumber.CreateChecked(-1f);
        var max = TNumber.CreateChecked(1f);

        _alpha = TNumber.CreateChecked(0.01f); // Leaky ReLU alpha
        _min = TNumber.CreateChecked(-1f); // Hard Tanh min
        _max = TNumber.CreateChecked(1f); // Hard Tanh max

        _tensor = Tensor.Random.Uniform(Shape, min, max, seed: 123456).ToTensor();
        _result = Tensor.Zeros<TNumber>(Shape);
    }

    [Benchmark]
    public void ReLU()
    {
        TensorBackend.ActivationFunctions.ReLU(_tensor, _result);
    }

    [Benchmark]
    public void ReLUBackward()
    {
        TensorBackend.ActivationFunctions.ReLUBackward(_tensor, _result);
    }

    [Benchmark]
    public void LeakyReLU()
    {
        TensorBackend.ActivationFunctions.LeakyReLU(_tensor, _result, _alpha);
    }

    [Benchmark]
    public void LeakyReLUBackward()
    {
        TensorBackend.ActivationFunctions.LeakyReLUBackward(_tensor, _result, _alpha);
    }

    [Benchmark]
    public void SoftSign()
    {
        TensorBackend.ActivationFunctions.SoftSign(_tensor, _result);
    }

    [Benchmark]
    public void SoftSignBackward()
    {
        TensorBackend.ActivationFunctions.SoftSignBackward(_tensor, _result);
    }

    [Benchmark]
    public void HardSigmoid()
    {
        TensorBackend.ActivationFunctions.HardSigmoid(_tensor, _result);
    }

    [Benchmark]
    public void HardSigmoidBackward()
    {
        TensorBackend.ActivationFunctions.HardSigmoidBackward(_tensor, _result);
    }

    [Benchmark]
    public void HardTanh()
    {
        TensorBackend.ActivationFunctions.HardTanh(_tensor, _result, _min, _max);
    }

    [Benchmark]
    public void HardTanhBackward()
    {
        TensorBackend.ActivationFunctions.HardTanhBackward(_tensor, _result, _min, _max);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _tensor.Dispose();
        _result.Dispose();
    }
}