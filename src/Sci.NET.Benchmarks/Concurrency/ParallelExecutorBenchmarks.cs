// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Concurrency;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Random;

namespace Sci.NET.Benchmarks.Concurrency;

[Config(typeof(PipeProfilerConfig))]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Benchmark")]
public class ParallelExecutorBenchmarks
{
    [Params(32768, 131072, 524288)]
    public int VectorSize { get; set; }

    private SystemMemoryBlock<float> _leftVector = null!;
    private SystemMemoryBlock<float> _rightVector = null!;
    private SystemMemoryBlock<float> _resultVector = null!;
    private ParallelExecutorThreadPool _parallelExecutorThreadPool = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _leftVector = new SystemMemoryBlock<float>(VectorSize);
        _rightVector = new SystemMemoryBlock<float>(VectorSize);
        _resultVector = new SystemMemoryBlock<float>(VectorSize);

        Prng.Instance.FillUniform(_leftVector, 0.0f, 1.0f);
        Prng.Instance.FillUniform(_rightVector, 0.0f, 1.0f);

        _parallelExecutorThreadPool = new ParallelExecutorThreadPool(Environment.ProcessorCount);
    }

    [Benchmark]
    public unsafe void ParallelExecutorFor()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopState
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ManagedTensorBackend.ParallelExecutor.For(
            0L,
            VectorSize,
            numWorkers,
            innerLoopState,
            InnerLoop);
    }

    [Benchmark]
    public unsafe void ThreadPoolForDontPreferLocal()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopState
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ParallelUtils.ThreadPoolFor(
            0,
            VectorSize,
            numWorkers,
            false,
            innerLoopState,
            InnerLoop);
    }

    [Benchmark]
    public unsafe void ThreadPoolForPreferLocal()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopState
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ParallelUtils.ThreadPoolFor(
            0,
            VectorSize,
            numWorkers,
            true,
            innerLoopState,
            InnerLoop);
    }

    [Benchmark]
    public unsafe void ThreadPoolReferenceTypeForDontPreferLocal()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopStateReferenceType
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ParallelUtils.ThreadPoolForReferenceType(
            0,
            VectorSize,
            numWorkers,
            false,
            innerLoopState,
            InnerLoop);
    }

    [Benchmark]
    public unsafe void ThreadPoolReferenceTypeForPreferLocal()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopStateReferenceType
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ParallelUtils.ThreadPoolForReferenceType(
            0,
            VectorSize,
            numWorkers,
            true,
            innerLoopState,
            InnerLoop);
    }

    [Benchmark]
    public unsafe void TplFor()
    {
        var numWorkers = ManagedTensorBackend.GetNumThreadsByElementCount<float>(VectorSize);

        var innerLoopState = new InnerLoopState
        {
            LeftPtr = _leftVector.ToPointer(),
            RightPtr = _rightVector.ToPointer(),
            ResultPtr = _resultVector.ToPointer()
        };

        ParallelUtils.TplFor(0, _leftVector.Length, numWorkers, innerLoopState, InnerLoop);
    }

    private static unsafe void InnerLoop(long idx, InnerLoopState state)
    {
        state.ResultPtr[idx] = state.LeftPtr[idx] * state.RightPtr[idx];
    }

    private static unsafe void InnerLoop(long idx, InnerLoopStateReferenceType state)
    {
        state.ResultPtr[idx] = state.LeftPtr[idx] * state.RightPtr[idx];
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _leftVector.Dispose();
        _rightVector.Dispose();
        _resultVector.Dispose();

        _parallelExecutorThreadPool.Dispose();
    }
}