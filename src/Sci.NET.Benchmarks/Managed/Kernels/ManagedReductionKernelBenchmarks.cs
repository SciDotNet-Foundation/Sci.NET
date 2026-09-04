// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedReductionKernelBenchmarks<TNumber> : BaseManagedBenchmark
    where TNumber : unmanaged, INumber<TNumber>
{
    [ParamsSource(nameof(ShapeOptions))]
    public Shape Shape { get; set; } = default!;

    [ParamsSource(nameof(ReductionAxesOptions))]
    public string ReductionAxes { get; set; } = default!;

    public ICollection<Shape> ShapeOptions =>
    [
        new Shape(16, 16, 16, 16),
        new Shape(32, 32, 32, 32),
        new Shape(16, 16, 16, 16, 16),
        new Shape(32, 32, 32, 32, 32)
    ];

    public ICollection<string> ReductionAxesOptions =>
    [
        "All",
        "Innermost",
        "MiddleAxis",
        "MiddleAxes",
        "Outermost"
    ];

    private Tensor<TNumber> _tensor = default!;
    private ITensor<TNumber> _result = default!;
    private int[] _axes = default!;

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

        _axes = ReductionAxes switch
        {
            "All" => [],
            "Innermost" => [0],
            "MiddleAxis" => [Shape.Rank / 2],
            "MiddleAxes" => [.. Enumerable.Range(1, Shape.Rank - 2)],
            "Outermost" => [Shape.Rank - 1],
            _ => throw new InvalidOperationException()
        };

        _tensor = Tensor.Random.Uniform(Shape, min, max, seed: 123456).ToTensor();
        _result = Tensor.Zeros<TNumber>(CalculateResultShape(Shape.Dimensions, _axes));
    }

    [Benchmark]
    public void Sum()
    {
        TensorBackend.Reduction.ReduceAdd(_tensor, _axes, _result);
    }

    [Benchmark]
    public void Mean()
    {
        TensorBackend.Reduction.ReduceMean(_tensor, _axes, _result);
    }

    [Benchmark]
    public void MinAll()
    {
        TensorBackend.Reduction.ReduceMin(_tensor, _axes, _result);
    }

    [Benchmark]
    public void Max()
    {
        TensorBackend.Reduction.ReduceMax(_tensor, _axes, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _tensor.Dispose();
        _result.Dispose();
    }

    private static Shape CalculateResultShape(int[] shape, int[]? axes)
    {
        var axisSet = axes is not null ? [.. axes] : new HashSet<int>();

        var resultShapeDimensions = new int[shape.Length];

        for (var i = 0; i < shape.Length; i++)
        {
            resultShapeDimensions[i] = axisSet.Contains(i) ? 0 : shape[i];
        }

        return new Shape([.. resultShapeDimensions.Where(dim => dim != 0)]);
    }
}