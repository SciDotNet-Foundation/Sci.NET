// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Concurrency;

internal sealed class ParallelExecutorThreadPoolThreadDetails
{
    public required ParallelExecutorThreadPool ThreadPool { get; init; }

    public required int ThreadIdx { get; init; }
}
