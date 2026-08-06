// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal class ParallelExecutorTaskCollectionEnumerator<TIndex> : IEnumerator<ParallelExecutorTask<TIndex>>
    where TIndex : IBinaryInteger<TIndex>
{
    private readonly ParallelExecutorTaskCollection<TIndex> _collection;
    private readonly ParallelExecutorTask<TIndex>[] _items;
    private int _currentIndex;

    public ParallelExecutorTaskCollectionEnumerator(ParallelExecutorTask<TIndex>[] items, ParallelExecutorTaskCollection<TIndex> taskCollection)
    {
        _items = items;
        _collection = taskCollection;
    }

    public ParallelExecutorTask<TIndex> Current => _items[_currentIndex];

    object IEnumerator.Current => _items[_currentIndex];

    public bool MoveNext()
    {
        if (_currentIndex >= _items.Length)
        {
            return false;
        }

        if (_collection.IsDisposed)
        {
            return false;
        }

        _currentIndex++;

        return true;
    }

    public void Reset()
    {
        _currentIndex = 0;
    }

    public void Dispose()
    {
    }
}