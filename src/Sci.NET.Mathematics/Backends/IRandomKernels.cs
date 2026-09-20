// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends;

/// <summary>
/// An interface for random kernels.
/// </summary>
public interface IRandomKernels
{
    /// <summary>
    /// Seeds the random number generator with the specified value.
    /// </summary>
    /// <param name="value">The seed value.</param>
    public void Seed(ulong value);

    /// <summary>
    /// Creates an <see cref="ITensor{TNumber}"/> filled with random values from
    /// the specified generator.
    /// </summary>
    /// <param name="shape">The shape of the <see cref="ITensor{TNumber}"/> to create.</param>
    /// <param name="min">The minimum value to be generated.</param>
    /// <param name="max">The maximum value to be generated.</param>
    /// <param name="seed">The random seed.</param>
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
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
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
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
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
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
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
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
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
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
    /// <param name="device">The device on which to create the tensor. If null, the default device will be used.</param>
    /// <typeparam name="TNumber">The type of number to be generated.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> filled with random data.</returns>
    public ITensor<TNumber> HeNormal<TNumber>(Shape shape, int inputUnits, ulong? seed = null, IDevice? device = null)
        where TNumber : unmanaged, IFloatingPoint<TNumber>;
}
