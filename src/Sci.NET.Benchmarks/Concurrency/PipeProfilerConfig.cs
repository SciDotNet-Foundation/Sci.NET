// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Sci.NET.Benchmarks.Concurrency;

public class PipeProfilerConfig : ManualConfig
{
    public PipeProfilerConfig()
    {
        AddJob(Job.Default);

        const ClrTraceEventParser.Keywords eventTypes = ClrTraceEventParser.Keywords.Exception
                                                        | ClrTraceEventParser.Keywords.GC
                                                        | ClrTraceEventParser.Keywords.Jit
                                                        | ClrTraceEventParser.Keywords.JitTracing
                                                        | ClrTraceEventParser.Keywords.Loader
                                                        | ClrTraceEventParser.Keywords.NGen
                                                        | ClrTraceEventParser.Keywords.Threading;

        var providers = new[]
        {
            new EventPipeProvider(
                ClrTraceEventParser.ProviderName,
                EventLevel.Verbose,
                (long)eventTypes),
            new EventPipeProvider("System.Buffers.ArrayPoolEventSource", EventLevel.Informational, long.MaxValue),
            new EventPipeProvider("Sci.NET.Mathematics.ParallelExecutor", EventLevel.Informational, long.MaxValue),
            new EventPipeProvider("System.Threading.Tasks.Parallel.EventSource", EventLevel.Informational, long.MaxValue)
        };

        AddDiagnoser(new EventPipeProfiler(providers: providers));
        AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()));
        AddDiagnoser(new ThreadingDiagnoser(new ThreadingDiagnoserConfig()));
    }
}