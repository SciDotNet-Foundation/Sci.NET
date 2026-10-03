// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;

namespace Sci.NET.Mathematics.Tensors.Random;

/// <summary>
/// A service for generating random <see cref="ITensor{TNumber}"/>s.
/// </summary>
public interface IRandomService
{
    /// <summary>
    /// Generates a random <see cref="ITensor{TNumber}"/> with values drawn from a normal distribution.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to generate.</param>
    /// <param name="min">The minimum value of the distribution.</param>
    /// <param name="max">The maximum value of the distribution.</param>
    /// <param name="seed">The seed for the random number generator.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of the <see cref="ITensor{TNumber}"/> to generate.</typeparam>
    /// <returns>A random <see cref="ITensor{TNumber}"/> with values drawn from a normal distribution.</returns>
    public ITensor<TNumber> Uniform<TNumber>(Shape shape, TNumber min, TNumber max, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from
    /// a normal (Gaussian) distribution with the specified mean and standard deviation.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="mean">The mean of the normal distribution.</param>
    /// <param name="stdDev">The standard deviation of the normal distribution.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> Normal<TNumber>(Shape shape, TNumber mean, TNumber stdDev, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from a Xavier/Glorot uniform distribution.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="inputUnits">The number of input units.</param>
    /// <param name="outputUnits">The number of output units.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> XavierUniform<TNumber>(Shape shape, int inputUnits, int outputUnits, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from a Xavier/Glorot normal distribution.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="inputUnits">The number of input units.</param>
    /// <param name="outputUnits">The number of output units.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> XavierNormal<TNumber>(Shape shape, int inputUnits, int outputUnits, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from a He uniform distribution.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="inputUnits">The number of input units.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> HeUniform<TNumber>(Shape shape, int inputUnits, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from a He normal distribution.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="inputUnits">The number of input units.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device to generate the <see cref="ITensor{TNumber}"/> on.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> HeNormal<TNumber>(Shape shape, int inputUnits, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;

    /// <summary>
    /// Seeds the random number generator to the specified value. If no device is specified, the default device is used.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    /// <param name="device">The device to seed the random number generator on.</param>
    public void Seed(ulong seed, IDevice? device = null);
}
