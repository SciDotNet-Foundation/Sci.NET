// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Backends.Managed;

namespace Sci.NET.Mathematics.UnitTests.Backends.Managed.ManagedBackend;

public class GetNumThreadsByElementCountShould
{
    [Fact]
    public void ReturnOne_ForSmallElementCounts()
    {
        ManagedTensorBackend.GetNumThreadsByElementCount<int, float>(16).Should().Be(1);
        ManagedTensorBackend.GetNumThreadsByElementCount<float>(16L).Should().Be(1);
    }

    [Fact]
    public void GenericIndexOverload_MatchesLongOverload()
    {
        foreach (var elementCount in new[] { 1, 32_768, 131_072, 524_288, 8_388_608 })
        {
            var expected = ManagedTensorBackend.GetNumThreadsByElementCount<float>(elementCount);

            ManagedTensorBackend
                .GetNumThreadsByElementCount<int, float>(elementCount)
                .Should()
                .Be(expected, $"the generic overload should agree with the long overload for {elementCount} elements");
        }
    }

    [Fact]
    public void GenericIndexOverload_ScalesWithElementCount()
    {
        var expected = (int)Math.Min(
            Math.Max(1L, 524_288L * sizeof(float) / ManagedTensorBackend.MinBytesPerThread),
            ManagedTensorBackend.MaxDegreeOfParallelism);

        ManagedTensorBackend.GetNumThreadsByElementCount<int, float>(524_288).Should().Be(expected);
    }
}
