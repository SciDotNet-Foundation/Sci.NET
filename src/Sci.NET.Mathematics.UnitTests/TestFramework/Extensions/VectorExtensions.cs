// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Runtime.Intrinsics;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Extensions;

/// <summary>
/// Provides extension methods for Vector256.
/// </summary>
public static class VectorExtensions
{
    /// <summary>
    /// Converts a Vector256 to an array.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <typeparam name="T">The type of the elements in the vector.</typeparam>
    /// <returns>>An array containing the elements of the vector.</returns>
    public static T[] ToArray<T>(this Vector256<T> vector)
        where T : struct
    {
        var array = new T[Vector256<T>.Count];

        for (int i = 0; i < Vector256<T>.Count; i++)
        {
            array[i] = vector.GetElement(i);
        }

        return array;
    }
}