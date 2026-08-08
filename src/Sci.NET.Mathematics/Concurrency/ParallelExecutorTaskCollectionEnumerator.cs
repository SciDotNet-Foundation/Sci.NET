// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal sealed class ParallelExecutorTaskCollectionEnumerator<TIndex> : IEnumerator<ParallelExecutorTask<TIndex>>
    where TIndex : IBinaryInteger<TIndex>
{
    private readonly ParallelExecutorTaskCollection<TIndex> _collection;
    private readonly ParallelExecutorTask<TIndex>[] _items;
    private int _currentIndex;

    public ParallelExecutorTaskCollectionEnumerator(ParallelExecutorTask<TIndex>[] items, ParallelExecutorTaskCollection<TIndex> taskCollection)
    {
        _items = items;
        _collection = taskCollection;
        _currentIndex = -1;
    }

    public ParallelExecutorTask<TIndex> Current => _items[_currentIndex];

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (_collection.IsDisposed)
        {
            return false;
        }

        if (_currentIndex + 1 >= _items.Length)
        {
            return false;
        }

        _currentIndex++;

        return true;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }

    public void Dispose()
    {
    }
}
