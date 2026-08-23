// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Sci.NET.Mathematics.Concurrency;

internal static partial class ProcessorAffinity
{
    public static void TrySetForCurrentThread(int cpuIndex)
    {
        if (cpuIndex < 0)
        {
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                _ = SetThreadIdealProcessor(GetCurrentThread(), (uint)cpuIndex);
            }
            else if (OperatingSystem.IsLinux())
            {
                SetLinuxAffinity(cpuIndex);
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
        }
    }

    private static void SetLinuxAffinity(int cpuIndex)
    {
        // cpu_set_t is a bitmask indexed by CPU; size it generously so high core indices fit.
        // Up to 1024 logical processors.
        const int maskBytes = 128;
        var mask = new byte[maskBytes];
        mask[cpuIndex / 8] = (byte)(1 << (cpuIndex % 8));

        _ = sched_setaffinity(0, maskBytes, mask);
    }

    [LibraryImport("kernel32.dll")]
    private static partial nint GetCurrentThread();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint SetThreadIdealProcessor(nint hThread, uint dwIdealProcessor);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int sched_setaffinity(int pid, nint cpusetsize, byte[] mask);
}
