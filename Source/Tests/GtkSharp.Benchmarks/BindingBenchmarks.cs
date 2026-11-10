using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Gtk;

namespace GtkSharp.Benchmarks;

/// <summary>
/// Detailed measurements for the hot paths of the bindings. The xUnit
/// performance tests are the gate; these benchmarks are for analysis and for
/// comparing revisions on the same machine.
///
/// GTK needs a display, so run them under Xvfb:
/// <code>xvfb-run -a dotnet run -c Release --project Source/Tests/GtkSharp.Benchmarks</code>
///
/// Use <c>--filter "*Label*"</c> to run a subset and <c>--job Dry</c> for a
/// quick smoke run.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class BindingBenchmarks
{
    private Label _label = null!;
    private Button _button = null!;
    private EventHandler _handler = null!;
    private ListStore _store = null!;

    [GlobalSetup]
    public void Setup()
    {
        Gtk.Application.Init();

        _label = new Label("x");
        _button = Button.NewWithLabel("click");
        _handler = static (_, _) => { };
        _store = new ListStore(GLib.GType.Int);

        for (var i = 0; i < 1000; i++)
            _store.AppendValues(i);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _label.Dispose();
        _button.Dispose();
        _store.Dispose();
    }

    [Benchmark]
    public void LabelCreateDispose()
    {
        using var label = new Label("x");
    }

    [Benchmark]
    public void LabelTextSet() => _label.Text = "some text";

    [Benchmark]
    public string LabelTextGet() => _label.Text;

    [Benchmark]
    public void GLibValueStringBoxing()
    {
        using var value = new GLib.Value("hello");
    }

    [Benchmark]
    public void SignalConnectDisconnect()
    {
        _button.Clicked += _handler;
        _button.Clicked -= _handler;
    }

    [Benchmark]
    public void SignalEmit() => GLib.Signal.Emit(_button, "clicked");

    [Benchmark]
    public void ModelIteration()
    {
        var valid = _store.GetIterFirst(out var iter);
        while (valid)
        {
            _store.GetValue(iter, 0);
            valid = _store.IterNext(ref iter);
        }
    }

    [Benchmark]
    public void WidgetPropertyRoundTrip() => _button.Sensitive = !_button.Sensitive;
}
