---
_layout: landing
title: Sci.NET Documentation
---

# Sci.NET

**Sci.NET** is a scientific computing library for .NET with a familiar API inspired by NumPy and PyTorch.
It provides N-dimensional tensor operations, broadcasting, linear algebra, and automatic differentiation — all
in idiomatic, strongly-typed C#.

> [!WARNING]
> Sci.NET is in **early development** and is not yet ready for production use. The public API may change between
> releases. Currently only the managed CPU backend is supported.

## Quick links

- [Getting Started](articles/getting-started.md) — install the package and create your first tensor
- [Core Concepts](articles/core-concepts.md) — tensors, shapes, devices, and memory
- [API Reference](api/index.md) — the full type and member reference

## Features

| Feature | Description |
| --- | --- |
| **Tensor operations** | Create and manipulate N-dimensional tensors with a fluent, generic API |
| **Broadcasting** | Automatic shape broadcasting for element-wise operations |
| **Linear algebra** | Matrix multiplication, contractions, and reductions |
| **Trigonometry** | A full suite of trigonometric and exponential functions |
| **Automatic differentiation** | Reverse-mode autograd for gradient computation (Preview) |
| **Extensible backends** | Pluggable compute-backend architecture (Managed CPU today, CUDA planned) |

## Example

```csharp
using Sci.NET.Mathematics.Tensors;

using var a = Tensor.FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } });
using var b = Tensor.FromArray<int>(new int[,] { { 5, 6 }, { 7, 8 } });
using var result = a.Add(b);

Console.WriteLine(result); // [[6, 8], [10, 12]]
```

## License

Sci.NET is licensed under the [Apache License 2.0](https://github.com/SciDotNet-Foundation/Sci.NET/blob/main/LICENSE).
Some packages include third-party components with their own licensing requirements.
