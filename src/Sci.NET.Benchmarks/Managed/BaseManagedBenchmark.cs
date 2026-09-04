// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Sci.NET.Benchmarks.Managed.Devices;
using Sci.NET.Mathematics.Backends;
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Benchmarks.Managed;

[MaxIterationCount(4096)]
public abstract class BaseManagedBenchmark
{
    public ITensorBackend TensorBackend { get; private set; } = null!;

    [Params(true, false)]
    public bool AvxEnabled { get; set; } = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        if (!AvxEnabled)
        {
            TensorBackend = NoAvxManagedTensorBackend.Instance;
            Tensor.SetDefaultBackend<NoAvxManagedTensorBackend>();
        }
        else
        {
            TensorBackend = ManagedTensorBackend.Instance;
            Tensor.SetDefaultBackend<ManagedTensorBackend>();
        }

        SetupBenchmark();
    }

    protected abstract void SetupBenchmark();
}