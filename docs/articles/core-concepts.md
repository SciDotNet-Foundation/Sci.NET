# Core Concepts

This article introduces the building blocks of Sci.NET: tensors, shapes, the rank-specific tensor types,
devices, and memory management.

## Tensors

A **tensor** is an N-dimensional array of numbers. In Sci.NET every tensor is represented by the generic
`ITensor<TNumber>` interface, where `TNumber` is any numeric type that implements .NET's generic-math
interfaces (`int`, `float`, `double`, `Sci.NET.Mathematics.Numerics.BFloat16`, and so on).

```csharp
using Sci.NET.Mathematics.Tensors;

using var tensor = Tensor.FromArray<float>(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });
```

## Shape

A tensor's **shape** describes the size of each dimension. The tensor above has shape `(2, 3)` — two rows and
three columns. The number of dimensions is the tensor's **rank**.

Sci.NET provides strongly-typed views for the common low ranks:

| Type | Rank | Created with |
| --- | --- | --- |
| `Scalar<TNumber>` | 0 | `tensor.ToScalar()` |
| `Vector<TNumber>` | 1 | `tensor.ToVector()` |
| `Matrix<TNumber>` | 2 | `tensor.ToMatrix()` |
| `ITensor<TNumber>` | N | `Tensor.FromArray(...)` |

Converting to the right type unlocks operations that only make sense at that rank — for example
`MatrixMultiply` is defined on `Matrix<TNumber>`:

```csharp
using var matrix = Tensor
    .FromArray<int>(new int[,] { { 1, 2 }, { 3, 4 } })
    .ToMatrix();
```

## Devices and backends

Every tensor lives on a **device** (`IDevice`) and is computed by a **backend** (`ITensorBackend`). Today the
managed CPU backend (`CpuComputeDevice` / `ManagedTensorBackend`) is the only supported backend; a CUDA backend
is planned. See [Backends](backends.md) for details on selecting and configuring backends.

## Memory management

Tensors wrap **unmanaged memory** so they can interoperate with native compute backends efficiently. Because
that memory is not tracked by the garbage collector, tensors implement `IDisposable`.

```csharp
using var a = Tensor.FromArray<int>(new[] { 1, 2, 3 });
// a.Dispose() is called automatically at the end of the scope
```

> [!IMPORTANT]
> Always dispose tensors — with a `using` statement or an explicit `Dispose()` call. Operations such as `Add`
> return **new** tensors that also need disposing.

## Next steps

- [Arithmetic](arithmetic.md)
- [Linear Algebra](linear-algebra.md)
- [Backends](backends.md)
