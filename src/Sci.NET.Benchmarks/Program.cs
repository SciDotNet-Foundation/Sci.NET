// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Sci.NET.Benchmarks.Concurrency;
using Sci.NET.Benchmarks.Managed;

var config = DefaultConfig.Instance;

// Linear algebra benchmarks
BenchmarkRunner.Run<ManagedMatrixMultiplyBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedMatrixMultiplyBenchmarks<double>>(config);
BenchmarkRunner.Run<ManagedInnerProductBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedInnerProductBenchmarks<double>>(config);
BenchmarkRunner.Run<ManagedContractionBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedContractionBenchmarks<double>>(config);
BenchmarkRunner.Run<ManagedHypotBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedHypotBenchmarks<double>>(config);

// Reduction benchmarks
BenchmarkRunner.Run<ManagedReductionBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedReductionBenchmarks<double>>(config);

// Arithmetic benchmarks
BenchmarkRunner.Run<ManagedBinaryArithmeticBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedBinaryArithmeticBenchmarks<double>>(config);
BenchmarkRunner.Run<ManagedUnaryArithmeticBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedUnaryArithmeticBenchmarks<double>>(config);

// Activation function benchmarks
BenchmarkRunner.Run<ManagedActivationFunctionBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedActivationFunctionBenchmarks<double>>(config);

// Broadcasting benchmarks
BenchmarkRunner.Run<ManagedBroadcastingBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedBroadcastingBenchmarks<double>>(config);

// Equality benchmarks
BenchmarkRunner.Run<ManagedEqualityBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedEqualityBenchmarks<double>>(config);

// Reshape benchmarks
BenchmarkRunner.Run<ManagedPermutationBenchmarks<float>>(config);
BenchmarkRunner.Run<ManagedPermutationBenchmarks<double>>(config);

// Parallel
BenchmarkRunner.Run<ParallelExecutorBenchmarks>(config);