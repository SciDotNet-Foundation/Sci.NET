# Linear Algebra

Sci.NET supports the core linear-algebra operations you need for scientific computing and machine learning:
matrix multiplication, tensor contractions, and reductions.

## Matrix multiplication

`MatrixMultiply` is defined on `Matrix<TNumber>`, so convert your rank-2 tensors with `ToMatrix()` first:

```csharp
using Sci.NET.Mathematics.Tensors;

using var matrixA = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } }).ToMatrix();
using var matrixB = Tensor.FromArray<int>(new int[,] { { 5, 6 }, { 7, 8 } }).ToMatrix();

using var result = matrixA.MatrixMultiply(matrixB);
Console.WriteLine(result); // [[19, 22], [43, 50]]
```

## Contractions

For higher-rank tensors, `Contract` generalises matrix multiplication by summing over one or more pairs of
axes — equivalent to a tensor dot product:

```csharp
using var a = Tensor.FromArray<float>(new float[,] { { 1, 2 }, { 3, 4 } });
using var b = Tensor.FromArray<float>(new float[,] { { 5, 6 }, { 7, 8 } });

// Contract the last axis of `a` with the first axis of `b`.
using var contracted = a.Contract(b, new[] { 1 }, new[] { 0 });
```

## Reductions

Reductions collapse a tensor along one or more axes. `Sum` accepts the axes to reduce and an optional
`keepDims` flag:

```csharp
using var tensor = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });

using var total = tensor.Sum();            // reduce all axes -> 10
using var sumAxis0 = tensor.Sum(new[] { 0 }); // [4, 6]
using var sumAxis1 = tensor.Sum(new[] { 1 }); // [3, 7]
```

Pass `keepDims: true` to retain reduced axes as size-1 dimensions, which is useful for broadcasting the result
back against the original tensor:

```csharp
using var kept = tensor.Sum(new[] { 1 }, keepDims: true); // shape (2, 1)
```

## Next steps

- [Automatic Differentiation](autograd.md) — backpropagate through linear-algebra operations
- [API Reference](../api/index.md)
