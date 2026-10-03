// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.UnitTests.Tensors.Manipulation.Broadcasting;

public class CanBroadcastShould
{
    public static readonly IEnumerable<object[]> MemberData = GetMemberData();

    private static IEnumerable<object[]> GetMemberData()
    {
        yield return [Shape.Scalar(), Shape.Scalar(), true];
        yield return [Shape.Scalar(), Shape.Vector(5), true];
        yield return [Shape.Scalar(), Shape.Matrix(5, 5), true];
        yield return [Shape.Scalar(), Shape.Tensor(5, 5, 5), true];

        yield return [Shape.Vector(5), Shape.Scalar(), false];
        yield return [Shape.Vector(1), Shape.Vector(5), true];
        yield return [Shape.Vector(5), Shape.Vector(5), true];
        yield return [Shape.Vector(5), Shape.Matrix(5, 5), true];
        yield return [Shape.Vector(4), Shape.Matrix(5, 5), false];
        yield return [Shape.Vector(5), Shape.Tensor(5, 5, 5), true];
        yield return [Shape.Vector(4), Shape.Tensor(5, 5, 5), false];

        yield return [Shape.Matrix(5, 5), Shape.Scalar(), false];
        yield return [Shape.Matrix(5, 5), Shape.Vector(5), false];
        yield return [Shape.Matrix(5, 5), Shape.Matrix(5, 5), true];
        yield return [Shape.Matrix(4, 5), Shape.Matrix(5, 5), false];
        yield return [Shape.Matrix(5, 5), Shape.Tensor(5, 5, 5), true];
        yield return [Shape.Matrix(5, 5), Shape.Tensor(5, 6, 5), false];
        yield return [Shape.Matrix(6, 5), Shape.Tensor(5, 5, 5), false];

        yield return [Shape.Tensor(5, 5, 5), Shape.Scalar(), false];
        yield return [Shape.Tensor(5, 5, 5), Shape.Vector(5), false];
        yield return [Shape.Tensor(5, 5, 5), Shape.Matrix(5, 5), false];
        yield return [Shape.Tensor(5, 5, 5), Shape.Tensor(5, 5, 5), true];
        yield return [Shape.Tensor(5, 6, 5), Shape.Tensor(5, 5, 5), false];
        yield return [Shape.Tensor(5, 5, 5), Shape.Tensor(6, 5, 5, 5), true];
        yield return [Shape.Tensor(5, 5, 5), Shape.Tensor(5, 6, 5, 5), false];
    }

    [Theory]
    [MemberData(nameof(MemberData))]
    public void ReturnExpectedResult_GivenTwoShapes(Shape original, Shape target, bool expected)
    {
        // Arrange
        var tensor = Tensor.Ones<int>(original);

        // Act
        var actual = tensor.CanBroadcastTo(target);

        // Assert
        actual.Should().Be(expected);
    }
}
