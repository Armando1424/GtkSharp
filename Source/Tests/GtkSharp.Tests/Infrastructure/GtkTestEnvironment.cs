using System;
using System.Threading;
using Xunit;

namespace GtkSharp.Tests.Infrastructure;

/// <summary>
/// Detects whether a GTK display is available and initializes GTK exactly once
/// per test process.
/// </summary>
/// <remarks>
/// GTK needs a display server. On CI (and on headless servers) the test run is
/// wrapped in <c>xvfb-run</c>. When no display is available the GTK based tests
/// are skipped instead of failing, so that the pure GLib/Cairo/Pango tests can
/// still run.
/// </remarks>
public static class GtkTestEnvironment
{
    private static readonly Lazy<ProbeResult> Result = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool IsAvailable => Result.Value.Available;

    public static string UnavailableReason => Result.Value.Reason
        ?? "GTK is not available in this process.";

    private static ProbeResult Probe()
    {
        try
        {
            var args = Array.Empty<string>();

            if (Gtk.Application.InitCheck("GtkSharp.Tests", ref args))
                return new ProbeResult(true, null);

            return new ProbeResult(
                false,
                "GTK could not open a display. Run the tests under a virtual display: " +
                "`xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release`.");
        }
        catch (Exception ex) when (
            ex is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException)
        {
            return new ProbeResult(false, $"GTK native libraries could not be loaded: {ex.Message}");
        }
    }

    private readonly record struct ProbeResult(bool Available, string? Reason);
}
