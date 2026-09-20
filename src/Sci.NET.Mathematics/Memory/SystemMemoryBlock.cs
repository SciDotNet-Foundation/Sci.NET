// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sci.NET.Mathematics.Comparison;
using Sci.NET.Mathematics.Intrinsics;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Memory;

/// <summary>
/// A <see cref="IMemoryBlock{T}"/> implementation for system memory.
/// </summary>
/// <typeparam name="T">The type of elements stored in the <see cref="IMemoryBlock{T}"/>.</typeparam>
[DebuggerDisplay("{ToString(),raw}")]
[DebuggerTypeProxy(typeof(SystemMemoryBlockDebugView<>))]
public sealed class SystemMemoryBlock<T> : IMemoryBlock<T>, IEquatable<SystemMemoryBlock<T>>
    where T : unmanaged
{
    private readonly bool _cannotDispose;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemMemoryBlock{T}"/> class.
    /// </summary>
    /// <param name="array">The array to create the span from.</param>
    public unsafe SystemMemoryBlock(T[] array)
        : this(array.LongLength)
    {
        Buffer.MemoryCopy(
            Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array)),
            Pointer,
            array.LongLength * Unsafe.SizeOf<T>(),
            array.LongLength * Unsafe.SizeOf<T>());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemMemoryBlock{T}"/> class.
    /// </summary>
    /// <param name="count">The number of elements to allocate.</param>
    public unsafe SystemMemoryBlock(long count)
    {
        var length = (nuint)count;
        var elementSize = (nuint)Unsafe.SizeOf<T>();
        var totalSize = length * elementSize;

        Pointer = (T*)NativeMemory.AlignedAlloc(totalSize, IntrinsicsHelper.CalculateRequiredAlignment());
        NativeMemory.Clear(Pointer, totalSize);
        Length = count;
    }

    private unsafe SystemMemoryBlock(T* pointer, long length)
    {
        Pointer = pointer;
        _cannotDispose = true;
        Length = length;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SystemMemoryBlock{T}"/> class.
    /// </summary>
    ~SystemMemoryBlock()
    {
        if (!IsDisposed)
        {
            Dispose(false);
        }
    }

    /// <inheritdoc />
    public long Length { get; }

    /// <inheritdoc />
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the pointer to the memory block.
    /// </summary>
#pragma warning disable CA1720 // Identifiers should not contain type names
    public unsafe T* Pointer { get; }
#pragma warning restore CA1720

    /// <summary>
    /// Gets a reference to the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element.</param>
    /// <exception cref="ArgumentOutOfRangeException">The specified index was out of range.</exception>
    /// <exception cref="ObjectDisposedException">Throws when the memory block has been disposed.</exception>
    public unsafe ref T this[long index]
    {
        [MethodImpl(ImplementationOptions.HotPath)]
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length);

            return ref Unsafe.Add(ref Unsafe.AsRef<T>(Pointer), (nint)index);
        }
    }

    /// <summary>
    /// Determines if the left and right <see cref="SystemMemoryBlock{T}"/>s are equal.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>A value indicating whether the two operands are equal.</returns>
    public static bool operator ==(SystemMemoryBlock<T> left, SystemMemoryBlock<T> right)
    {
        ObjectDisposedException.ThrowIf(left.IsDisposed, left);
        ObjectDisposedException.ThrowIf(right.IsDisposed, right);

        return left.Equals(right);
    }

    /// <summary>
    /// Determines if the left and right <see cref="SystemMemoryBlock{T}"/>s are not equal.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>A value indicating whether the two operands are equal.</returns>
    public static bool operator !=(SystemMemoryBlock<T> left, SystemMemoryBlock<T> right)
    {
        ObjectDisposedException.ThrowIf(left.IsDisposed, left);
        ObjectDisposedException.ThrowIf(right.IsDisposed, right);

        return !left.Equals(right);
    }

    /// <summary>
    /// Fills the <see cref="SystemMemoryBlock{T}"/> with the specified values.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="buffer">The values to add to the buffer.</param>
    /// <param name="bytesToCopy">The number of bytes to copy.</param>
    /// <exception cref="ObjectDisposedException">Throws when the object has already been disposed.</exception>
    public unsafe void FillBytes(long start, byte[] buffer, long bytesToCopy)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var bufferPtr = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(buffer));
        var dataPtr = Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(Pointer), (nuint)start));

        Buffer.MemoryCopy(bufferPtr, dataPtr, Length * Unsafe.SizeOf<T>(), bytesToCopy);
    }

    /// <inheritdoc />
    public unsafe T[] ToArray()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (Length == 0)
        {
            return [];
        }

        var result = new T[Length];

        Buffer.MemoryCopy(
            Pointer,
            Unsafe.AsPointer(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(result), 0)),
            Length * Unsafe.SizeOf<T>(),
            Length * Unsafe.SizeOf<T>());

        return result;
    }

    /// <inheritdoc />
    public unsafe void Fill(T value)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        for (var i = 0L; i < Length; i++)
        {
            Unsafe.Add(ref Unsafe.AsRef<T>(Pointer), (nuint)i) = value;
        }
    }

    /// <inheritdoc />
    public unsafe void CopyFromSystemMemory(SystemMemoryBlock<T> source)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (source.Length != Length)
        {
            throw new ArgumentException("Source must have the same length as the destination.", nameof(source));
        }

        Buffer.MemoryCopy(source.Pointer, Pointer, Length * Unsafe.SizeOf<T>(), Length * Unsafe.SizeOf<T>());
    }

    /// <inheritdoc />
    public unsafe void CopyFrom(T[] array)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (array.Length != Length)
        {
            throw new ArgumentException("Array must have the same length as the source.", nameof(array));
        }

        if (Length == 0)
        {
            return;
        }

        Buffer.MemoryCopy(
            Unsafe.AsPointer(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), 0)),
            Pointer,
            Length * Unsafe.SizeOf<T>(),
            Length * Unsafe.SizeOf<T>());
    }

    /// <inheritdoc />
    public unsafe void WriteTo(Stream stream)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var byteLength = Length * Unsafe.SizeOf<T>();

        // Write the entire block in one go if possible
        if (byteLength <= int.MaxValue)
        {
            stream.Write(new ReadOnlySpan<byte>(Pointer, (int)byteLength).ToArray());
            return;
        }

        // Otherwise write in chunks using long pointer
        // Note: There is no test coverage for this code path as no in-memory stream can exceed 2.5GB.
        //       We could add a test for this by creating a custom stream that allows for a larger buffer size
        //       or by using a file stream, but this would slow down CI builds.
        var remaining = byteLength;
        var pointer = (byte*)Pointer;
        var offset = 0L;

        while (remaining > 0)
        {
            var chunkLength = Math.Min(remaining, int.MaxValue - 1);
            stream.Write(new ReadOnlySpan<byte>(pointer + offset, (int)chunkLength));
            remaining -= chunkLength;
            offset += chunkLength;
        }
    }

    /// <summary>
    /// Reads the elements from the stream into the <see cref="SystemMemoryBlock{T}"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="startOffset">The offset to start reading from.</param>
    /// <param name="storedDataLength">The length of the stored data.</param>
    /// <exception cref="ArgumentException">The stored data length must be the same as the length of the memory block.</exception>
    public unsafe void ReadElementsFrom(Stream stream, long startOffset, long storedDataLength)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        var byteLength = Length * Unsafe.SizeOf<T>();

        if (byteLength != storedDataLength)
        {
            throw new ArgumentException(
                "The stored data length must be the same as the length of the memory block.",
                nameof(storedDataLength));
        }

        _ = stream.Seek(startOffset, SeekOrigin.Begin);

        if (byteLength <= int.MaxValue)
        {
            _ = stream.Read(new Span<byte>(Pointer, (int)byteLength));
            return;
        }

        var remaining = byteLength;
        var pointer = (byte*)Pointer;
        var offset = 0L;

        while (remaining > 0)
        {
            var chunkLength = Math.Min(remaining, int.MaxValue - 1);
            _ = stream.Read(new Span<byte>(pointer + offset, (int)chunkLength));
            remaining -= chunkLength;
            offset += chunkLength;
        }
    }

    /// <inheritdoc />
    public unsafe void BlockCopyFrom(IMemoryBlock<T> handle, long srcIdx, long dstIdx, long count)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThan(srcIdx, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(dstIdx, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(srcIdx, handle.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dstIdx, Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, handle.Length - srcIdx);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Length - dstIdx);

        if (handle is not SystemMemoryBlock<T> memoryBlock)
        {
            throw new InvalidOperationException(
                $"Cannot copy from {handle.GetType().Name} to {typeof(SystemMemoryBlock<T>).Name}.");
        }

        Buffer.MemoryCopy(
            memoryBlock.Pointer + srcIdx,
            Pointer + dstIdx,
            count * Unsafe.SizeOf<T>(),
            count * Unsafe.SizeOf<T>());
    }

    /// <inheritdoc />
    public unsafe void BlockCopyFrom(Span<byte> buffer, int srcIdx, int dstIdx, int count)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThan(srcIdx, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(dstIdx, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(srcIdx, buffer.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dstIdx, Length * Unsafe.SizeOf<T>());
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, buffer.Length - srcIdx);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, (Length * Unsafe.SizeOf<T>()) - dstIdx);

        var byteDestination = (byte*)ToPointer() + dstIdx;

        fixed (byte* byteSource = buffer)
        {
            Buffer.MemoryCopy(byteSource + srcIdx, byteDestination, count, count);
        }
    }

    /// <inheritdoc />
    public void UnsafeFreeMemory()
    {
        ReleaseUnmanagedResources();
        IsDisposed = true;
    }

    /// <inheritdoc />
    public IMemoryBlock<T> Copy()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var result = new SystemMemoryBlock<T>(Length);
        CopyTo(result);
        return result;
    }

    /// <inheritdoc />
    public SystemMemoryBlock<T> ToSystemMemory()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return this;
    }

    /// <inheritdoc />
    public unsafe void CopyTo(IMemoryBlock<T> destination)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (destination is not SystemMemoryBlock<T> systemMemoryBlock)
        {
            throw new ArgumentException($"Destination must be a {nameof(SystemMemoryBlock<>)}.", nameof(destination));
        }

        if (destination.Length != Length)
        {
            throw new ArgumentException("Destination must have the same length as the source.", nameof(destination));
        }

        Buffer.MemoryCopy(Pointer, systemMemoryBlock.Pointer, Length * Unsafe.SizeOf<T>(), Length * Unsafe.SizeOf<T>());
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return obj is SystemMemoryBlock<T> other && Equals(other);
    }

    /// <inheritdoc cref="IValueEquatable{T}.Equals(T)" />
    public unsafe bool Equals([NotNullWhen(true)] SystemMemoryBlock<T>? other)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ObjectDisposedException.ThrowIf(other?.IsDisposed ?? false, other ?? this);

        return other is not null && Pointer == other.Pointer && Length == other.Length;
    }

    /// <inheritdoc cref="IValueEquatable{T}.GetHashCode" />
    public override unsafe int GetHashCode()
    {
        return HashCode.Combine((int)((long)Pointer & uint.MaxValue), (int)((long)Pointer >> 32));
    }

    /// <summary>
    /// For <see cref="Span{Char}"/>, returns a string representation of the <see cref="SystemMemoryBlock{T}"/>,
    /// otherwise, returns the name of the type and the length of the <see cref="SystemMemoryBlock{T}"/>.
    /// </summary>
    /// <returns>
    /// A string representation of the <see cref="SystemMemoryBlock{T}"/>,
    /// otherwise, the name of the type and the length of the <see cref="SystemMemoryBlock{T}"/>.
    /// </returns>
    public override unsafe string ToString()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return typeof(T) == typeof(char)
            ? new string(new ReadOnlySpan<char>(Pointer, checked((int)Length)))
            : $"{nameof(SystemMemoryBlock<>)}<{typeof(T).Name}>[{Length}]";
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that iterates over the collection.</returns>
    /// <exception cref="ObjectDisposedException">The memory block has been disposed.</exception>
    public SystemMemoryBlockEnumerator<T> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return new SystemMemoryBlockEnumerator<T>(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets a reference to the <see cref="SystemMemoryBlock{T}"/>.
    /// </summary>
    /// <returns>A reference to the <see cref="SystemMemoryBlock{T}"/>.</returns>
    /// <exception cref="ObjectDisposedException">The memory block has been disposed.</exception>
    public unsafe ref T GetReference()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ref Unsafe.AsRef<T>(Pointer);
    }

    /// <summary>
    /// Gets a reference to the <see cref="SystemMemoryBlock{T}"/> at the specified index.
    /// </summary>
    /// <returns>A reference to the first element of the <see cref="SystemMemoryBlock{T}"/>.</returns>
    /// <exception cref="ObjectDisposedException">The memory block has been disposed.</exception>
    public unsafe T* ToPointer()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return Pointer;
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> to the <see cref="SystemMemoryBlock{T}"/>.
    /// </summary>
    /// <returns>A span to this instance.</returns>
    /// <exception cref="InvalidOperationException">The length of the <see cref="SystemMemoryBlock{T}"/> is too big to create a <see cref="Span{T}"/>.</exception>
    /// <exception cref="ObjectDisposedException">The memory block has been disposed.</exception>
    public unsafe Span<T> AsSpan()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (Length > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Cannot create a span larger than int.MaxValue ({int.MaxValue}) elements.");
        }

        return new Span<T>(Pointer, (int)Length);
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> to the <see cref="SystemMemoryBlock{T}"/> at the specified index with the specified length.
    /// </summary>
    /// <param name="index">The index to start the span at.</param>
    /// <param name="length">The length of the span.</param>
    /// <returns>A span to this instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The specified index was out of range.</exception>
    public unsafe Span<T> AsSpan(long index, int length)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Length - length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Length - index);

        return new Span<T>(Pointer + index, length);
    }

    /// <summary>
    /// Loads a vector from the memory block at the specified index.
    /// </summary>
    /// <param name="i">The index to load the vector from.</param>
    /// <returns>The loaded vector.</returns>
    [MethodImpl(ImplementationOptions.HotPath)]
    public unsafe Vector<T> LoadVector(long i)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(i, Length - Vector<T>.Count);
        ArgumentOutOfRangeException.ThrowIfLessThan(i, 0);

        return Vector.Load(Pointer + i);
    }

    /// <summary>
    /// Unsafely loads a vector from the memory block at the specified index without bounds checking.
    /// </summary>
    /// <param name="i">The index to load the vector from.</param>
    /// <returns>The loaded vector.</returns>
    [MethodImpl(ImplementationOptions.HotPath)]
    public unsafe Vector<T> UnsafeLoadUncheckedVector(long i)
    {
        return Vector.Load(Pointer + i);
    }

    /// <summary>
    /// Stores a vector to the memory block at the specified index.
    /// </summary>
    /// <param name="i">The index to store the vector to.</param>
    /// <param name="vector">>The vector to store.</param>
    [MethodImpl(ImplementationOptions.HotPath)]
    public unsafe void StoreVector(long i, Vector<T> vector)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(i, Length - Vector<T>.Count);
        ArgumentOutOfRangeException.ThrowIfLessThan(i, 0);

        vector.Store(Pointer + i);
    }

    /// <summary>
    /// Unsafely stores a vector to the memory block at the specified index without bounds checking.
    /// </summary>
    /// <param name="i">The index to store the vector to.</param>
    /// <param name="vector">>The vector to store.</param>
    [MethodImpl(ImplementationOptions.HotPath)]
    public unsafe void UnsafeStoreUncheckedVector(long i, Vector<T> vector)
    {
        vector.Store(Pointer + i);
    }

    /// <summary>
    /// Disposes the <see cref="SystemMemoryBlock{T}"/>.
    /// </summary>
    /// <param name="isDisposing">A value indicating if the instance is disposing.</param>
    private void Dispose(bool isDisposing)
    {
        ReleaseUnmanagedResources();

        if (!IsDisposed && isDisposing)
        {
            IsDisposed = true;
        }
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        if (_cannotDispose || IsDisposed)
        {
            return;
        }

        NativeMemory.AlignedFree(Pointer);
    }
}
