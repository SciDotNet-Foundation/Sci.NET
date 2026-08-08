// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Concurrency;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Random;

namespace Sci.NET.Benchmarks.Concurrency;

[MaxIterationCount(4096)]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Benchmark")]
public class ParallelExecutorBenchmarks
{
    [Params(256, 512, 1024, 248)] public int VectorSize { get; set; }

    private SystemMemoryBlock<float> _leftVector = null!;
    private SystemMemoryBlock<float> _rightVector = null!;
    private SystemMemoryBlock<float> _resultVector = null!;
    private ParallelExecutor _parallelExecutor = null!;
    private ParallelExecutorThreadPool _threadPool = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _leftVector = new SystemMemoryBlock<float>(VectorSize);
        _rightVector = new SystemMemoryBlock<float>(VectorSize);
        _resultVector = new SystemMemoryBlock<float>(VectorSize);

        Prng.Instance.FillUniform(_leftVector, 0.0f, 1.0f);
        Prng.Instance.FillUniform(_rightVector, 0.0f, 1.0f);

        _threadPool = new ParallelExecutorThreadPool(Environment.ProcessorCount);
        _parallelExecutor = new ParallelExecutor(_threadPool);
    }

    [Benchmark]
    public unsafe void UsingNewParallel()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numWorkers,
            idx =>
            {
                InnerLoop(
                    idx,
                    VectorSize,
                    numWorkers,
                    _leftVector.ToPointer(),
                    _rightVector.ToPointer(),
                    _resultVector.ToPointer());
            });

        _parallelExecutor.Run(tasks);
    }

    [Benchmark]
    public unsafe void UsingOldParallel()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        Parallel.For(
            0,
            numWorkers,
            idx =>
            {
                InnerLoop(
                    idx,
                    VectorSize,
                    numWorkers,
                    _leftVector.ToPointer(),
                    _rightVector.ToPointer(),
                    _resultVector.ToPointer());
            });
    }

    private static unsafe void InnerLoop(
        long tid,
        long n,
        long processes,
        float* leftPtr,
        float* rightPtr,
        float* resultPtr)
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            resultPtr[start + i] = leftPtr[start + i] + rightPtr[start + i];
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _leftVector.Dispose();
        _rightVector.Dispose();
        _resultVector.Dispose();

        _threadPool.Dispose();
    }
}