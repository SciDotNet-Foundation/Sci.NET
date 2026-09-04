// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed.Kernels;

public class ManagedInnerProductKernelBenchmarks<TNumber> : BaseManagedBenchmark
    where TNumber : unmanaged, INumber<TNumber>
{
    [Params(5000, 10000, 16384, 32768)]
    public int Length { get; set; }

    private Mathematics.Tensors.Vector<TNumber> _left = default!;
    private Mathematics.Tensors.Vector<TNumber> _right = default!;
    private Scalar<TNumber> _result = default!;

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

        _left = Tensor.Random.Uniform(new Shape(Length), min, max, seed: 123456).ToVector();
        _right = Tensor.Random.Uniform(new Shape(Length), min, max, seed: 654321).ToVector();
        _result = Tensor.Zeros<TNumber>(Shape.Scalar()).ToScalar();
    }

    [Benchmark]
    public void Inner()
    {
        TensorBackend.LinearAlgebra.InnerProduct(_left, _right, _result);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _left.Dispose();
        _right.Dispose();
        _result.Dispose();
    }
}