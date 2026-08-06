// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedBinaryOperationIterator
{
    public static unsafe void Apply<TOp, TNumber>(TNumber* leftPtr, TNumber* rightPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TOp : IBinaryOperation<TNumber>, IBinaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingBinaryOperationIterator.Apply<TOp, TNumber>(leftPtr, rightPtr, resultPtr, n, device);
    }
}