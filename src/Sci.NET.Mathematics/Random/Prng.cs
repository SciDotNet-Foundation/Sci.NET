// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Numerics;

namespace Sci.NET.Mathematics.Random;

/// <summary>
/// Random number generation.
/// </summary>
/// <remarks>Derived from mostly SplitMix64-based RNGs.</remarks>
[PublicAPI]
public class Prng
{
    private ulong _seed;
    private ulong _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="Prng"/> class.
    /// </summary>
    public Prng()
    {
        // Use a secure RNG to generate the seed.
        _seed = BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Prng"/> class with the specified seed.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    public Prng(ulong seed)
    {
        _seed = seed;
    }

    /// <summary>
    /// Gets singleton instance of the <see cref="Prng"/> class.
    /// </summary>
    public static Prng Instance { get; } = new();

    /// <summary>
    /// Generates a uniformly distributed float in [0, 1).
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    /// <param name="index">The index of the random number to generate.</param>
    /// <param name="stream">The stream identifier.</param>
    /// <returns>A uniformly distributed float in [0, 1).</returns>
    public static float Uniform01F(ulong seed, long index, ulong stream)
    {
        var bits = Hash32(seed, index, stream);
        return (bits >> 8) * (1.0f / 16_777_216.0f);
    }

    /// <summary>
    /// Generates a uniformly distributed double in [0, 1).
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    /// <param name="index">The index of the random number to generate.</param>
    /// <param name="stream">The stream identifier.</param>
    /// <returns>A uniformly distributed double in [0, 1).</returns>
    public static double Uniform01D(ulong seed, long index, ulong stream)
    {
        var bits = Hash64(seed, index, stream);
        return (bits >> 11) * (1.0 / 9007199254740992.0);
    }

    /// <summary>
    /// Generates a normally distributed float with mean 0 and standard deviation 1 using the Box-Muller transform.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    /// <param name="index">The index of the random number to generate.</param>
    /// <param name="stream">The stream identifier.</param>
    /// <returns>A normally distributed float with mean 0 and standard deviation 1.</returns>
    public static float NormalF(ulong seed, long index, ulong stream)
    {
        // Box-Muller transform
        var pair = index >> 1;
        var u1 = Uniform01F(seed, 2 * pair, stream);
        var u2 = Uniform01F(seed, (2 * pair) + 1, stream);
        var r = float.Sqrt(-2f * float.Log(u1 + float.Epsilon));
        var theta = 2f * float.Pi * u2;
        var (z0, z1) = float.SinCos(theta);
        z0 *= r;
        z1 *= r;
        return (index & 1) == 0 ? z0 : z1;
    }

    /// <summary>
    /// Generates a normally distributed double with mean 0 and standard deviation 1 using the Box-Muller transform.
    /// </summary>
    /// <param name="seed">The seed to use.</param>
    /// <param name="index">The index of the random number to generate.</param>
    /// <param name="stream">The stream identifier.</param>
    /// <returns>A normally distributed double with mean 0 and standard deviation 1.</returns>
    public static double NormalD(ulong seed, long index, ulong stream)
    {
        // Box-Muller transform
        var pair = index >> 1;
        var u1 = Uniform01D(seed, 2 * pair, stream);
        var u2 = Uniform01D(seed, (2 * pair) + 1, stream);
        var r = double.Sqrt(-2.0 * double.Log(u1 + double.Epsilon));
        var theta = 2.0 * double.Pi * u2;
        var (z0, z1) = double.SinCos(theta);
        z0 *= r;
        z1 *= r;
        return (index & 1) == 0 ? z0 : z1;
    }

    /// <summary>
    /// Sets the global seed for random number generation.
    /// </summary>
    /// <param name="seed">The seed to set.</param>
    public void SetSeed(ulong seed)
    {
        _seed = seed;
        _stream = 0;
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random numbers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<BFloat16> dst, BFloat16 min, BFloat16 max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);

        Fill(
            dst,
            i =>
            {
                var u = Uniform01F(_seed, i, streamId);
                var fMin = (float)min;
                var fMax = (float)max;
                var r = fMin + ((fMax - fMin) * u);
                return float.Clamp(r, fMin, fMax);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random numbers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<Half> dst, Half min, Half max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);

        Fill(
            dst,
            i =>
            {
                var u = Uniform01F(_seed, i, streamId);
                var fMin = (float)min;
                var fMax = (float)max;
                var r = fMin + ((fMax - fMin) * u);
                return (Half)float.Clamp(r, fMin, fMax);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random numbers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<float> dst, float min, float max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i => min + ((max - min) * Uniform01F(_seed, i, streamId)));
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random numbers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<double> dst, double min, double max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i => min + ((max - min) * Uniform01D(_seed, i, streamId)));
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<int> dst, int min, int max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (uint)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return min + (int)r;
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<uint> dst, uint min, uint max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = max - min;

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return min + r;
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<long> dst, long min, long max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (ulong)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange64(_seed, i, streamId, range);
                return min + (long)r;
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<ulong> dst, ulong min, ulong max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = max - min;

        Fill(
            dst,
            i =>
            {
                var r = UniformRange64(_seed, i, streamId, range);
                return min + r;
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<short> dst, short min, short max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (uint)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return (short)(min + (int)r);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<ushort> dst, ushort min, ushort max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (uint)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return (ushort)(min + r);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<sbyte> dst, sbyte min, sbyte max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (uint)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return (sbyte)(min + (int)r);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with uniformly distributed random integers in the range [min, max].
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillUniform(SystemMemoryBlock<byte> dst, byte min, byte max, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var range = (uint)(max - min);

        Fill(
            dst,
            i =>
            {
                var r = UniformRange32(_seed, i, streamId, range);
                return (byte)(min + r);
            });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with normally distributed random numbers with specified mean and standard deviation.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="mean">The mean of the normal distribution.</param>
    /// <param name="std">The standard deviation of the normal distribution.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillNormal(SystemMemoryBlock<BFloat16> dst, BFloat16 mean, BFloat16 std, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i =>
        {
            var meanF = (float)mean;
            var stdF = (float)std;
            var normal = NormalF(_seed, i, streamId);
            return meanF + (stdF * normal);
        });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with normally distributed random numbers with specified mean and standard deviation.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="mean">The mean of the normal distribution.</param>
    /// <param name="std">The standard deviation of the normal distribution.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillNormal(SystemMemoryBlock<Half> dst, Half mean, Half std, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i =>
        {
            var meanF = (float)mean;
            var stdF = (float)std;
            var normal = NormalF(_seed, i, streamId);
            return (Half)(meanF + (stdF * normal));
        });
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with normally distributed random numbers with specified mean and standard deviation.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="mean">The mean of the normal distribution.</param>
    /// <param name="std">The standard deviation of the normal distribution.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillNormal(SystemMemoryBlock<float> dst, float mean, float std, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i => mean + (std * NormalF(_seed, i, streamId)));
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with normally distributed random numbers with specified mean and standard deviation.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="mean">The mean of the normal distribution.</param>
    /// <param name="std">The standard deviation of the normal distribution.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillNormal(SystemMemoryBlock<double> dst, double mean, double std, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        Fill(dst, i => mean + (std * NormalD(_seed, i, streamId)));
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierUniform(SystemMemoryBlock<BFloat16> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = BFloat16.Sqrt(6f / (fanIn + fanOut));
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierUniform(SystemMemoryBlock<Half> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = Half.Sqrt((Half)(6f / (fanIn + fanOut)));
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierUniform(SystemMemoryBlock<float> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = float.Sqrt(6f / (fanIn + fanOut));
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierUniform(SystemMemoryBlock<double> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = double.Sqrt(6.0 / (fanIn + fanOut));
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierNormal(SystemMemoryBlock<BFloat16> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = float.Sqrt(2f / (fanIn + fanOut));
        FillNormal(dst, 0f, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierNormal(SystemMemoryBlock<Half> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = (Half)float.Sqrt(2f / (fanIn + fanOut));
        FillNormal(dst, (Half)0f, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierNormal(SystemMemoryBlock<float> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = float.Sqrt(2f / (fanIn + fanOut));
        FillNormal(dst, 0f, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the Xavier/Glorot normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="fanOut">The number of output units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillXavierNormal(SystemMemoryBlock<double> dst, long fanIn, long fanOut, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = double.Sqrt(2.0 / (fanIn + fanOut));
        FillNormal(dst, 0.0, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeUniform(SystemMemoryBlock<BFloat16> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = BFloat16.Sqrt(6f / fanIn);
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeUniform(SystemMemoryBlock<Half> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = Half.Sqrt((Half)(6f / fanIn));
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeUniform(SystemMemoryBlock<float> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = float.Sqrt(6f / fanIn);
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He uniform initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeUniform(SystemMemoryBlock<double> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var a = double.Sqrt(6.0 / fanIn);
        FillUniform(dst, -a, a, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeNormal(SystemMemoryBlock<BFloat16> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = BFloat16.Sqrt(2f / fanIn);
        FillNormal(dst, 0f, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeNormal(SystemMemoryBlock<Half> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = float.Sqrt(2f / fanIn);
        FillNormal(dst, (Half)0f, (Half)std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeNormal(SystemMemoryBlock<float> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = float.Sqrt(2f / fanIn);
        FillNormal(dst, 0f, std, streamId);
    }

    /// <summary>
    /// Fills the destination <see cref="IMemoryBlock{T}"/> with values according to the He normal initialization scheme.
    /// </summary>
    /// <param name="dst">The destination <see cref="IMemoryBlock{T}"/> to fill.</param>
    /// <param name="fanIn">The number of input units.</param>
    /// <param name="stream">The stream identifier.</param>
    public void FillHeNormal(SystemMemoryBlock<double> dst, long fanIn, ulong stream = 0)
    {
        var streamId = GetNextStream(stream);
        var std = double.Sqrt(2.0 / fanIn);
        FillNormal(dst, 0.0, std, streamId);
    }

    private static void Fill<T>(SystemMemoryBlock<T> dst, Func<long, T> gen)
        where T : unmanaged
    {
        var numThreads = ManagedTensorBackend.GetNumThreadsByElementCount<T>(dst.Length);

        var chunk = dst.Length / numThreads;
        var remainder = dst.Length % numThreads;

        _ = Parallel.For(0, dst.Length, tid =>
        {
            var start = (tid * chunk) + Math.Min(tid, remainder);
            var count = tid < remainder ? chunk + 1 : chunk;

            for (var i = 0; i < count; i++)
            {
                dst[i] = gen(start + i);
            }
        });
    }

    private static ulong Mix64(ulong z)
    {
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    private static uint Hash32(ulong seed, long index, ulong stream)
    {
        unchecked
        {
            // Use the upper 32 bits of the 64-bit hash as they have better statistical quality.
            return (uint)(Hash64(seed, index, stream) >> 32);
        }
    }

    private static ulong Hash64(ulong seed, long index, ulong stream)
    {
        unchecked
        {
            var x = 0x9E3779B97F4A7C15UL * (ulong)index;
            x += Mix64(seed);
            x ^= Mix64(stream);
            return Mix64(x);
        }
    }

    [SuppressMessage("Roslynator", "RCS1252:Normalize usage of infinite loop", Justification = "Readability.")]
    private static ulong UniformRange64(ulong seed, long index, ulong stream, ulong s)
    {
        // Based on "Fast Random Integer Generation in an Interval" by Daniel Lemire
        ArgumentOutOfRangeException.ThrowIfZero(s);
        var threshold = unchecked((0UL - s) % s);

        while (true)
        {
            var x = Hash64(seed, index, stream);
            UInt128 m = (UInt128)x * s;
            var low = (ulong)m;
            var high = (ulong)(m >> 64);

            if (low >= threshold)
            {
                return high;
            }

            index++;
        }
    }

    [SuppressMessage("Roslynator", "RCS1252:Normalize usage of infinite loop", Justification = "Readability.")]
    private static uint UniformRange32(ulong seed, long index, ulong stream, uint s)
    {
        // Based on "Fast Random Integer Generation in an Interval" by Daniel Lemire
        ArgumentOutOfRangeException.ThrowIfZero(s);
        var threshold = unchecked((0u - s) % s);

        while (true)
        {
            var x = Hash32(seed, index, stream);
            var m = (ulong)x * s;
            var low = (uint)m;
            var high = (uint)(m >> 32);

            if (low >= threshold)
            {
                return high;
            }

            index++;
        }
    }

    private ulong GetNextStream(ulong streamId)
    {
        if (streamId != 0)
        {
            return streamId;
        }

        return Interlocked.Increment(ref _stream);
    }
}