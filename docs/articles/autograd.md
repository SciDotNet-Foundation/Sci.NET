# Automatic Differentiation

Sci.NET includes **reverse-mode automatic differentiation** (autograd), which records the operations performed
on tensors into a computation graph and computes gradients by backpropagation.

> [!WARNING]
> Autograd is a **preview feature** and is a work in progress. Its API and behaviour may change. You must
> explicitly opt in before using it (see below).

## Enabling autograd

Autograd is gated behind a preview-feature flag. Enable it once at startup:

```csharp
using Sci.NET.Mathematics;

SciDotNetConfiguration.PreviewFeatures.EnableAutoGrad();
```

## Tracking gradients

Mark the tensors you want to differentiate with respect to by requesting a gradient. You can do this when
creating a tensor with the `requiresGradient` parameter, or afterwards with `WithGradient()`:

```csharp
using Sci.NET.Mathematics.Tensors;

using var x = Tensor.FromArray<float>(
    new float[] { 1f, 2f, 3f },
    requiresGradient: true);

// or, from an existing tensor:
using var y = someTensor.WithGradient();
```

Operations on gradient-tracking tensors record themselves into the computation graph automatically.

## Computing gradients

Call `Backward()` on the output tensor to propagate gradients back through the graph. The gradient of each
tracked tensor is then available through its `Gradient` property:

```csharp
using var x = Tensor.FromArray<float>(new float[] { 1f, 2f, 3f }, requiresGradient: true);

// Build an expression: z = sum(x * x)
using var squared = x.Multiply(x);
using var z = squared.Sum();

z.Backward();

// dz/dx = 2x -> [2, 4, 6]
Console.WriteLine(x.Gradient);
```

Whether a tensor carries a gradient is exposed by `RequiresGradient`, and `Gradient` is guaranteed non-null
when it is `true`.

## Clearing gradients

Between training iterations, reset accumulated gradients with `ClearGradient()`:

```csharp
x.ClearGradient();
```

## Next steps

- [Arithmetic](arithmetic.md) and [Linear Algebra](linear-algebra.md) — the operations you differentiate through
- [API Reference](../api/Sci.NET.Mathematics.SciDotNetConfiguration.PreviewFeatures.yml)
