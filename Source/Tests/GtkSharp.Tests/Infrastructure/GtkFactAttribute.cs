using System;
using Xunit;

namespace GtkSharp.Tests.Infrastructure;

/// <summary>
/// A <see cref="FactAttribute"/> for tests that need a real GTK display.
/// The test is reported as skipped (not failed) when GTK cannot be initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class GtkFactAttribute : FactAttribute
{
    public GtkFactAttribute()
    {
        if (!GtkTestEnvironment.IsAvailable)
            Skip = GtkTestEnvironment.UnavailableReason;
    }
}
