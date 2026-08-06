// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.UnitTests.Concurrency;

public class ParallelExecutorTests
{
    [Fact]
    public void For_SetsTheValuesCorrectly()
    {
        // Arrange
        var threadPool = new ParallelExecutorThreadPool(Environment.ProcessorCount, ThreadPriority.Normal);
        var parallelExecutor = new ParallelExecutor(threadPool);

        var array = new int[128];

        parallelExecutor.For(
            1,
            128,
            1,
            idx => array[idx] = idx);
    }
}