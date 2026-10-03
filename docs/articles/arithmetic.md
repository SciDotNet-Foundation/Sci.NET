# Arithmetic

Sci.NET provides element-wise arithmetic, exponential, and trigonometric operations over tensors of any rank.
Each operation returns a **new** tensor, so remember to dispose the results.

## Element-wise operations

```csharp
using Sci.NET.Mathematics.Tensors;

using var a = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });
using var b = Tensor.FromArray<int>(new int[,] { { 5, 6 }, { 7, 8 } });

using var sum = a.Add(b);        // [[6, 8], [10, 12]]
using var diff = b.Subtract(a);  // [[4, 4], [4, 4]]
using var prod = a.Multiply(b);  // [[5, 12], [21, 32]]
using var quot = b.Divide(a);    // element-wise division
using var neg = a.Negate();      // [[-1, -2], [-3, -4]]
using var root = a.Sqrt();       // element-wise square root
```

## Broadcasting

When operands have different but compatible shapes, Sci.NET **broadcasts** the smaller operand across the
larger one, following the same rules as NumPy and PyTorch. Dimensions are compatible when they are equal or one
of them is 1.

```csharp
using var matrix = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });
using var vector = Tensor.FromArray<int>(new int[] { 6, 6 });

using var result = matrix.Subtract(vector);
Console.WriteLine(result); // [[-5, -4], [-3, -2]]
```

Here the length-2 vector is broadcast across each row of the 2×2 matrix.

## Exponential and logarithmic functions

```csharp
using var x = Tensor.FromArray<float>(new float[] { 1f, 2f, 3f });

using var exp = x.Exp(); // e^x element-wise
using var log = x.Log(); // natural log element-wise
```

## Trigonometry

```csharp
using var angles = Tensor.FromArray<float>(new float[] { 0f, 1.5708f, 3.1416f });

using var sin = angles.Sin();
using var cos = angles.Cos();
using var tan = angles.Tan();
```

## Next steps

- [Linear Algebra](linear-algebra.md) — matrix multiplication, contractions, and reductions
- [Automatic Differentiation](autograd.md) — differentiate through these operations
