// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Sci.NET.Mathematics.Collections;

internal class PausableBoundedBlockingCollectionDebugView<T>
{
    private readonly PausableBoundedBlockingCollection<T> _blockingCollection;

    public PausableBoundedBlockingCollectionDebugView(PausableBoundedBlockingCollection<T> collection)
    {
        _blockingCollection = collection;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => [.. _blockingCollection];
}