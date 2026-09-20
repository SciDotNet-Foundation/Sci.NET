// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

// Nested types should not be visible
#pragma warning disable CA1034

namespace Sci.NET.Mathematics;

/// <summary>
/// Configuration for Sci.NET.
/// </summary>
public static class SciDotNetConfiguration
{
    /// <summary>
    /// Configuration for preview features.
    /// </summary>
    public static class PreviewFeatures
    {
        /// <summary>
        /// Gets a value indicating whether the auto-grad feature is enabled.
        /// </summary>
        public static bool AutoGradEnabled { get; private set; }

        /// <summary>
        /// Enables the auto-grad feature.
        /// </summary>
        public static void EnableAutoGrad()
        {
            AutoGradEnabled = true;
        }

        /// <summary>
        /// Names of preview features.
        /// </summary>
        public static class Names
        {
            /// <summary>
            /// The name of the auto-grad preview feature.
            /// </summary>
            public const string AutoGrad = "AutoGrad";
        }
    }
}

// Nested types should not be visible
#pragma warning restore CA1034
