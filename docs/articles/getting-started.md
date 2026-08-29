# Getting Started

This guide walks you through installing Sci.NET and creating your first tensor.

## Prerequisites

- **.NET 10 SDK** or later

Verify your SDK version with:

```bash
dotnet --version
```

## Installation

Add the `Sci.NET.Mathematics` package to your project:

```bash
dotnet add package Sci.NET.Mathematics
```

> [!NOTE]
> Sci.NET is in early development and published as a pre-release package. You may need to enable pre-release
> versions in your package manager.

## Your first tensor

Bring the tensor API into scope:

```csharp
using Sci.NET.Mathematics.Tensors;
```

Create a tensor from a multi-dimensional array:

```csharp
using var tensor = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });
Console.WriteLine(tensor); // [[1, 2], [3, 4]]
```

> [!IMPORTANT]
> Tensors implement `IDisposable` and manage unmanaged memory. Always use a `using` statement or call
> `Dispose()` when you are done with a tensor to avoid leaking native memory.

## Basic arithmetic

Element-wise operations return a new tensor:

```csharp
using var a = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });
using var b = Tensor.FromArray<int>(new int[,] { { 5, 6 }, { 7, 8 } });
using var sum = a.Add(b);

Console.WriteLine(sum); // [[6, 8], [10, 12]]
```

## Creating tensors other ways

```csharp
using var zeros = Tensor.Zeros<float>(2, 3);       // 2x3 tensor of 0
using var ones = Tensor.Ones<float>(2, 3);         // 2x3 tensor of 1
using var filled = Tensor.FillWith<float>(7f, 2, 2); // 2x2 tensor of 7
```

## Next steps

- [Core Concepts](core-concepts.md) — understand tensors, shapes, devices, and memory
- [Arithmetic](arithmetic.md) — element-wise math and broadcasting
- [Linear Algebra](linear-algebra.md) — matrix multiplication, contractions, and reductions
- [Automatic Differentiation](autograd.md) — compute gradients with reverse-mode autograd
