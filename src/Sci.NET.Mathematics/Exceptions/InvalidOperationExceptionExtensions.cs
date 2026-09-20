// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sci.NET.Mathematics.Exceptions;

/// <summary>
/// Provides helper methods to throw exceptions.
/// </summary>
public static class InvalidOperationExceptionExtensions
{
#pragma warning disable CA1034 // CA1034 doesn't seem to support extension members
    extension(InvalidOperationException)
#pragma warning restore CA1034
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> with a message that the object is disposed.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="expected">The other value to compare.</param>
        /// <param name="customMessage">The custom message to include in the exception.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="InvalidOperationException">The value of <paramref name="value"/> is not equal to the <paramref name="expected"/> value.</exception>
        [StackTraceHidden]
        [ExcludeFromCodeCoverage]
        public static void ThrowIfNotEqual<T>(T value, T expected, string customMessage)
            where T : IEquatable<T>
        {
            if (!value.Equals(expected))
            {
                throw new InvalidOperationException(customMessage);
            }
        }

        /// <summary>
        /// Throws a <see cref="InvalidOperationException"/> when the <paramref name="value"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="value">The value to check for null.</param>
        /// <param name="message">The exception message.</param>
        /// <typeparam name="T">The type of the value to check for null.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static void ThrowIfNull<T>([System.Diagnostics.CodeAnalysis.NotNull] T? value, string message)
        {
            if (value is null)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
