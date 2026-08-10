// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Benchmarks.Concurrency;

public class InnerLoopStateReferenceType
{
    public unsafe float* LeftPtr { get; init; }

    public unsafe float* RightPtr { get; init; }

    public unsafe float* ResultPtr { get; init; }
}