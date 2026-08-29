# Backends

A **backend** (`ITensorBackend`) provides the actual implementations of tensor operations for a particular
compute device. Sci.NET's backend architecture is pluggable, so the same tensor code can run on different
hardware as new backends become available.

## Available backends

| Backend | Device | Status |
| --- | --- | --- |
| `ManagedTensorBackend` | `CpuComputeDevice` | Supported |
| CUDA | GPU | Planned |

> [!NOTE]
> Today the managed CPU backend is the only working backend. It serves as the reference implementation and the
> regression baseline for future accelerated backends.

## The default backend

New tensors use the default backend, which is the managed CPU backend unless you change it:

```csharp
using Sci.NET.Mathematics.Tensors;

var backend = Tensor.DefaultBackend; // ManagedTensorBackend by default
```

## Changing the default backend

Call `SetDefaultBackend<TBackend>()` to make a different backend the default for subsequently created tensors.
The backend type must implement `ITensorBackend` and have a parameterless constructor:

```csharp
using Sci.NET.Mathematics.Backends.Managed;
using Sci.NET.Mathematics.Tensors;

Tensor.SetDefaultBackend<ManagedTensorBackend>();
```

## Per-tensor backends

Most factory methods accept an optional backend argument, so you can place individual tensors on a specific
backend without changing the global default:

```csharp
var backend = new ManagedTensorBackend();
using var tensor = Tensor.FromArray<int>(new int[] { 1, 2, 3 }, backend: backend);
```

## Next steps

- [Core Concepts](core-concepts.md) — how devices and memory relate to backends
- [API Reference](../api/Sci.NET.Mathematics.Backends.yml)
