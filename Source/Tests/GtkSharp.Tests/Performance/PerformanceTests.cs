using System.Diagnostics;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Performance;

/// <summary>
/// Performance and memory guard rails for the bindings.
///
/// These tests are not micro-benchmarks: the thresholds are intentionally
/// generous so they only fail on real regressions (leaks, accidental O(n^2)
/// loops, unbounded allocation) and not on CI noise. Detailed numbers belong in
/// Source/Tests/GtkSharp.Benchmarks.
///
/// Run them with:
/// <code>xvfb-run -a dotnet test --filter "Category=Performance"</code>
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "Performance")]
public sealed class PerformanceTests
{
    // Thresholds measured on a Raspberry Pi 4 (ARM64) and multiplied by 3-5x so
    // that shared CI runners and slower machines still pass.
    private const long LabelAllocationsPerOperation = 4 * 1024;
    private const long ValueAllocationsPerOperation = 512;
    private const long TextSetAllocationsPerOperation = 1024;
    private const int LabelCpuMillisecondsPer2000 = 3000;
    private const int ValueCpuMillisecondsPer50000 = 2000;
    private const int TextSetCpuMillisecondsPer10000 = 2000;
    private const int SignalCpuMillisecondsPer2000 = 2000;
    private const int EmitCpuMillisecondsPer10000 = 5000;
    private const int ModelIterationCpuMilliseconds = 5000;
    private const long WorkingSetGrowthBytes = 32L * 1024 * 1024;

    [GtkFact]
    public void LabelCreateDispose_ManagedAllocationsAreBounded()
    {
        var perOperation = MeasureAllocationsPerOperation(
            2000,
            static () => { using var label = new Label("x"); });

        Assert.True(
            perOperation < LabelAllocationsPerOperation,
            $"Label create/dispose allocates {perOperation} B/op (limit {LabelAllocationsPerOperation} B).");
    }

    [GtkFact]
    public void LabelCreateDispose_CpuIsBounded()
    {
        var elapsed = MeasureCpu(() =>
        {
            for (var i = 0; i < 2000; i++)
            {
                using var label = new Label("x");
            }
        });

        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(LabelCpuMillisecondsPer2000),
            $"2000 Label create/dispose cycles took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {LabelCpuMillisecondsPer2000} ms).");
    }

    [GtkFact]
    public void LabelCreateDispose_DoesNotGrowWorkingSet()
    {
        // Warm up GTK/GObject type registration and allocator caches.
        for (var i = 0; i < 500; i++)
        {
            using var warmup = new Label("warmup");
        }

        var before = StabilizedWorkingSet();

        for (var i = 0; i < 5000; i++)
        {
            using var label = new Label("drift");
        }

        var after = StabilizedWorkingSet();
        var growth = after - before;

        Assert.True(
            growth < WorkingSetGrowthBytes,
            $"Working set grew {growth / 1024 / 1024} MB after 5000 Label create/dispose cycles " +
            $"(limit {WorkingSetGrowthBytes / 1024 / 1024} MB).");
    }

    [Fact]
    public void GLibValueBoxing_ManagedAllocationsAreBounded()
    {
        var perOperation = MeasureAllocationsPerOperation(
            20000,
            static () => { using var value = new GLib.Value("hello"); });

        Assert.True(
            perOperation < ValueAllocationsPerOperation,
            $"GLib.Value string boxing allocates {perOperation} B/op (limit {ValueAllocationsPerOperation} B).");
    }

    [Fact]
    public void GLibValueBoxing_CpuIsBounded()
    {
        var elapsed = MeasureCpu(() =>
        {
            for (var i = 0; i < 50000; i++)
            {
                using var value = new GLib.Value("hello");
            }
        });

        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(ValueCpuMillisecondsPer50000),
            $"50000 GLib.Value boxes took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {ValueCpuMillisecondsPer50000} ms).");
    }

    [GtkFact]
    public void LabelTextSet_ManagedAllocationsAreBounded()
    {
        using var label = new Label("x");

        var perOperation = MeasureAllocationsPerOperation(
            10000,
            () => label.Text = "some text");

        Assert.True(
            perOperation < TextSetAllocationsPerOperation,
            $"Label.Text set allocates {perOperation} B/op (limit {TextSetAllocationsPerOperation} B).");
    }

    [GtkFact]
    public void LabelTextSet_CpuIsBounded()
    {
        using var label = new Label("x");

        var elapsed = MeasureCpu(() =>
        {
            for (var i = 0; i < 10000; i++)
                label.Text = "some text";
        });

        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(TextSetCpuMillisecondsPer10000),
            $"10000 Label.Text sets took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {TextSetCpuMillisecondsPer10000} ms).");
    }

    [GtkFact]
    public void SignalConnectDisconnect_CpuIsBounded()
    {
        using var button = Button.NewWithLabel("click");
        var handler = new EventHandler(static (_, _) => { });

        var elapsed = MeasureCpu(() =>
        {
            for (var i = 0; i < 2000; i++)
            {
                button.Clicked += handler;
                button.Clicked -= handler;
            }
        });

        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(SignalCpuMillisecondsPer2000),
            $"2000 signal connect/disconnect pairs took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {SignalCpuMillisecondsPer2000} ms).");
    }

    [GtkFact]
    public void SignalConnectDisconnect_DoesNotGrowWorkingSet()
    {
        using var button = Button.NewWithLabel("click");
        var handler = new EventHandler(static (_, _) => { });

        for (var i = 0; i < 500; i++)
        {
            button.Clicked += handler;
            button.Clicked -= handler;
        }

        var before = StabilizedWorkingSet();

        for (var cycle = 0; cycle < 5; cycle++)
        {
            for (var i = 0; i < 2000; i++)
            {
                button.Clicked += handler;
                button.Clicked -= handler;
            }
        }

        var after = StabilizedWorkingSet();
        var growth = after - before;

        Assert.True(
            growth < WorkingSetGrowthBytes,
            $"Working set grew {growth / 1024 / 1024} MB after 10000 signal connect/disconnect pairs " +
            $"(limit {WorkingSetGrowthBytes / 1024 / 1024} MB).");
    }

    [GtkFact]
    public void SignalEmit_CpuIsBounded()
    {
        using var button = Button.NewWithLabel("click");
        var clicks = 0;
        button.Clicked += (_, _) => clicks++;

        var elapsed = MeasureCpu(() =>
        {
            for (var i = 0; i < 10000; i++)
                GLib.Signal.Emit(button, "clicked");
        });

        Assert.True(clicks > 0);
        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(EmitCpuMillisecondsPer10000),
            $"10000 signal emissions took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {EmitCpuMillisecondsPer10000} ms).");
    }

    [GtkFact]
    public void ModelIteration_CpuIsBounded()
    {
        using var store = new ListStore(GLib.GType.Int);
        for (var i = 0; i < 5000; i++)
            store.AppendValues(i);

        var elapsed = MeasureCpu(() =>
        {
            for (var pass = 0; pass < 20; pass++)
            {
                var valid = store.GetIterFirst(out var iter);
                while (valid)
                {
                    store.GetValue(iter, 0);
                    valid = store.IterNext(ref iter);
                }
            }
        });

        Assert.True(
            elapsed < TimeSpan.FromMilliseconds(ModelIterationCpuMilliseconds),
            $"Iterating 5000 rows 20 times took {elapsed.TotalMilliseconds:F0} ms " +
            $"(limit {ModelIterationCpuMilliseconds} ms).");
    }

    [GtkFact]
    public void DisposedWidgets_ReleaseTheirNativeObjects()
    {
        // Warm up GTK internals so the registry only reflects our widgets.
        for (var i = 0; i < 200; i++)
        {
            using var warmup = new Label("warmup");
        }

        var before = CountTrackedObjects();

        for (var i = 0; i < 2000; i++)
        {
            using var label = new Label("disposed");
        }

        var after = CountTrackedObjects();
        var growth = after - before;

        Assert.True(
            growth < 200,
            $"The GLib object registry grew by {growth} entries ({before} -> {after}) " +
            "after 2000 disposed labels, which suggests a wrapper leak.");
    }

    [GtkFact]
    public void UndisposedWidgets_RemainInTheObjectRegistry()
    {
        var before = CountTrackedObjects();
        var leaked = new List<Label>();

        try
        {
            for (var i = 0; i < 100; i++)
                leaked.Add(new Label("leaked"));

            var after = CountTrackedObjects();

            // This test proves the counter really detects leaks; without it the
            // disposed-widgets test could pass for the wrong reason.
            Assert.True(
                after - before >= 100,
                $"The registry only grew by {after - before} entries after creating 100 labels.");
        }
        finally
        {
            foreach (var label in leaked)
                label.Dispose();
        }
    }

    /// <summary>
    /// Reads GLib.Object's internal wrapper registry. GtkSharp keeps a strong
    /// reference to every live wrapper through a toggle ref, so finalizers are
    /// not a reliable leak indicator for widgets; this registry is.
    /// </summary>
    private static int CountTrackedObjects()
    {
        var field = typeof(GLib.Object).GetField(
            "Objects",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(field);

        var registry = (System.Collections.IDictionary)field!.GetValue(null)!;
        lock (registry.SyncRoot)
            return registry.Count;
    }

    private static long MeasureAllocationsPerOperation(long iterations, System.Action action)
    {
        for (var i = 0; i < 20; i++)
            action();

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (long i = 0; i < iterations; i++)
            action();
        return (GC.GetAllocatedBytesForCurrentThread() - before) / iterations;
    }

    private static TimeSpan MeasureCpu(System.Action action)
    {
        // Warm up type registration and caches before measuring.
        action();

        var before = Process.GetCurrentProcess().TotalProcessorTime;
        action();
        return Process.GetCurrentProcess().TotalProcessorTime - before;
    }

    private static long StabilizedWorkingSet()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return Process.GetCurrentProcess().WorkingSet64;
    }
}
