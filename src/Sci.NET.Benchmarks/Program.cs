// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using Sci.NET.Benchmarks.Concurrency;
using Sci.NET.Benchmarks.Managed;
using Sci.NET.Benchmarks.Managed.Kernels;

// Linear algebra benchmarks
BenchmarkRunner.Run<ManagedMatrixMultiplyKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedMatrixMultiplyKernelBenchmarks<double>>();
BenchmarkRunner.Run<ManagedInnerProductKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedInnerProductKernelBenchmarks<double>>();
BenchmarkRunner.Run<ManagedContractionBenchmarks<float>>();
BenchmarkRunner.Run<ManagedContractionBenchmarks<double>>();
BenchmarkRunner.Run<ManagedHypotKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedHypotKernelBenchmarks<double>>();

// Reduction benchmarks
BenchmarkRunner.Run<ManagedReductionKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedReductionKernelBenchmarks<double>>();

// Arithmetic benchmarks
BenchmarkRunner.Run<ManagedBinaryArithmeticKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedBinaryArithmeticKernelBenchmarks<double>>();
BenchmarkRunner.Run<ManagedUnaryArithmeticKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedUnaryArithmeticKernelBenchmarks<double>>();

// Activation function benchmarks
BenchmarkRunner.Run<ManagedActivationFunctionKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedActivationFunctionKernelBenchmarks<double>>();

// Broadcasting benchmarks
BenchmarkRunner.Run<ManagedBroadcastingKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedBroadcastingKernelBenchmarks<double>>();

// Equality benchmarks
BenchmarkRunner.Run<ManagedEqualityKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedEqualityKernelBenchmarks<double>>();

// Reshape benchmarks
BenchmarkRunner.Run<ManagedPermutationKernelBenchmarks<float>>();
BenchmarkRunner.Run<ManagedPermutationKernelBenchmarks<double>>();

// Casting
BenchmarkRunner.Run<ManagedCastingKernelBenchmarks<float, double>>();
BenchmarkRunner.Run<ManagedCastingKernelBenchmarks<double, float>>();

// Parallel
BenchmarkRunner.Run<ParallelExecutorBenchmarks>();