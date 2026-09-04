// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedCastingKernelBenchmarks<TIn, TOut> : BaseManagedBenchmark
    where TIn : unmanaged, INumber<TIn>
    where TOut : unmanaged, INumber<TOut>
{
    [ParamsSource(nameof(ShapeOptions))]
    public Shape Shape { get; set; } = default!;

    public ICollection<Shape> ShapeOptions =>
    [
        new Shape(400, 200),
        new Shape(400, 200, 100),
        new Shape(400, 200, 100, 50),
    ];

    private Tensor<TIn> _tensor = default!;
    private Tensor<TOut> _result = default!;

    protected override void SetupBenchmark()
    {
        TIn min;
        TIn max;

        if (GenericMath.IsSigned<TIn>())
        {
            min = TIn.CreateChecked(-100f);
            max = TIn.CreateChecked(100f);
        }
        else
        {
            min = TIn.CreateChecked(0);
            max = TIn.CreateChecked(100f);
        }

        _tensor = Tensor.Random.Uniform(Shape, min, max, seed: 123456).ToTensor();
        _result = Tensor.Zeros<TOut>(_tensor.Shape).ToTensor();
    }

    [Benchmark]
    public void Cast()
    {
        TensorBackend.Casting.Cast(_tensor, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _tensor.Dispose();
        _result.Dispose();
    }
}