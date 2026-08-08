// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sci.NET.Mathematics.Exceptions;

/// <summary>
/// An exception thrown when attempting to access native memory which has
/// already been freed.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public class NativeMemoryAlreadyFreedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeMemoryAlreadyFreedException"/> class.
    /// </summary>
    public NativeMemoryAlreadyFreedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeMemoryAlreadyFreedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NativeMemoryAlreadyFreedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeMemoryAlreadyFreedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public NativeMemoryAlreadyFreedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Throws a <see cref="NativeMemoryAlreadyFreedException"/> when <paramref name="memoryPointer"/> is zero.
    /// </summary>
    /// <param name="memoryPointer">The pointer to validate.</param>
    /// <exception cref="NativeMemoryAlreadyFreedException">Thrown when <paramref name="memoryPointer"/> is zero.</exception>
    [StackTraceHidden]
    [ExcludeFromCodeCoverage]
    public static void ThrowIfNullPointer(UIntPtr memoryPointer)
    {
        if (memoryPointer == UIntPtr.Zero)
        {
            throw new NativeMemoryAlreadyFreedException("The native memory was already freed.");
        }
    }

    /// <summary>
    /// Throws a <see cref="NativeMemoryAlreadyFreedException"/> when <paramref name="memoryPointer"/> is <c>null</c>.
    /// </summary>
    /// <param name="memoryPointer">The pointer to validate.</param>
    /// <exception cref="NativeMemoryAlreadyFreedException">Thrown when <paramref name="memoryPointer"/> is <c>null</c>.</exception>
    [StackTraceHidden]
    [ExcludeFromCodeCoverage]
    public static unsafe void ThrowIfNullPointer(void* memoryPointer)
    {
        if (memoryPointer == (void*)0)
        {
            throw new NativeMemoryAlreadyFreedException("The native memory was already freed.");
        }
    }

    /// <summary>
    /// Throws a <see cref="NativeMemoryAlreadyFreedException"/> when <paramref name="memoryPointer"/> is <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The element type of the pointer.</typeparam>
    /// <param name="memoryPointer">The pointer to validate.</param>
    /// <exception cref="NativeMemoryAlreadyFreedException">Thrown when <paramref name="memoryPointer"/> is <c>null</c>.</exception>
    [StackTraceHidden]
    [ExcludeFromCodeCoverage]
    public static unsafe void ThrowIfNullPointer<T>(T* memoryPointer)
        where T : unmanaged
    {
        if (memoryPointer == (void*)0)
        {
            throw new NativeMemoryAlreadyFreedException("The native memory was already freed.");
        }
    }
}