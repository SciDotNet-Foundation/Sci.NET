// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Collections;

internal class DisposableArray<T> : IDisposable
    where T : IDisposable
{
    private readonly T[] _items;

    public DisposableArray(int count)
    {
        _items = new T[count];
    }

    ~DisposableArray()
    {
        Dispose(false);
    }

    public T this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public T[] AsArray()
    {
        return _items;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            foreach (var item in _items)
            {
                item.Dispose();
            }
        }
    }
}