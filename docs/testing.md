# Testing GtkSharp

This document explains how the test suite works and how to add tests that a
maintainer can trust.

## Quick start

```sh
dotnet tool restore
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release
```

Expected output on a healthy checkout:

```text
Passed! - Failed: 0, Passed: 191, Skipped: 0, Total: 191
```

Without a display, the GTK tests are skipped:

```text
Passed! - Failed: 0, Passed: <n>, Skipped: <gtk tests>, Total: 191
```

## Why GTK tests are different

GTK is a single threaded, display bound C library. Three consequences shape
this test suite:

1. **A display is required.** Widgets cannot be created without `gtk_init`
   succeeding. CI and headless machines use `xvfb-run`.
2. **No parallel execution.** xUnit runs test collections in parallel by
   default; GTK does not allow it. `AssemblyInfo.cs` disables parallelization
   for the whole assembly and the GTK tests share the `Gtk` collection.
3. **Everything must be disposed.** Widgets wrap native memory. Tests use
   `using var` so the wrapper is disposed even when an assertion fails.

## Infrastructure

| Type | Purpose |
|:--|:--|
| `GtkTestEnvironment` | Probes `gtk_init_check` once per process. Exposes `IsAvailable` and `UnavailableReason`. |
| `GtkFactAttribute` | `[GtkFact]` behaves like `[Fact]`, but reports *skipped* instead of failed when GTK cannot start. |
| `GtkCollection` / `GtkFixture` | Serializes all GTK tests and initializes GTK once before the first of them runs. |
| `AssemblyInfo.cs` | Disables test parallelization for the assembly. |

Pure GLib, Cairo and Pango tests use a normal `[Fact]` and run even without a
display.

## Adding a widget test

```csharp
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
public sealed class SwitchTests
{
    [GtkFact]
    public void Active_RoundTrips()
    {
        using var widget = new Switch();

        widget.Active = true;

        Assert.True(widget.Active);
    }
}
```

Rules:

* Inherit the collection: `[Collection(GtkCollection.Name)]`.
* Use `[GtkFact]`, not `[Fact]`.
* `using var` for every widget.
* One behaviour per test.

## Signals and events

Emit the signal synchronously and assert the managed handler ran. Do not spin
the main loop unless the signal is asynchronous.

```csharp
[GtkFact]
public void ButtonClick_FiresClicked()
{
    using var button = Button.NewWithLabel("ok");
    var fired = false;
    button.Clicked += (_, _) => fired = true;

    button.Click();               // gtk_button_clicked

    Assert.True(fired);
}
```

To emit by name (useful for signals without a typed helper):

```csharp
GLib.Signal.Emit(button, "clicked");
```

To assert that a handler was disconnected, remove it and emit again; the
counter must not increase.

## Marshalling

Round-trip values through a real widget and assert the value that comes back:

```csharp
using var entry = new Entry { Text = "Hola 🌍 こんにちは" };
Assert.Equal("Hola 🌍 こんにちは", entry.Text);
```

Cover strings (empty, long, unicode), `bool`, `int`, `double` and enums. These
are the properties most likely to break when bindings are regenerated.

## Native identity

GtkSharp may return a different managed wrapper for the same native object.
Never assert `Assert.Same` on widgets obtained from a getter. Compare handles:

```csharp
Assert.Equal(window.Handle, button.Parent!.Handle);
Assert.Contains(button.Handle, window.Children.Select(c => c.Handle));
```

## Rendering tests (no display)

Cairo and Pango do not need a display, so they use plain `[Fact]`:

```csharp
using var surface = new ImageSurface(Format.ARGB32, 64, 64);
using var context = new Cairo.Context(surface);
context.SetSourceRGB(1, 0, 0);
context.Paint();
surface.Flush();
surface.WriteToPng(path);
```

These catch interop problems in the low level libraries even when the GTK
parts are skipped.

## Running a subset

```sh
# One class
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests \
  --filter "FullyQualifiedName~SignalTests"

# One test
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests \
  --filter "DisplayName~ToggleButtonClick_TogglesActiveAndFiresToggled"

# Detailed output
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests \
  --logger "console;verbosity=detailed"
```

## Debugging

| Problem | Action |
|:--|:--|
| `cannot open display` | Prefix the command with `xvfb-run -a`. |
| Tests pass but GTK ones are skipped | Check `GtkTestEnvironment.UnavailableReason` in the test output; you are probably running headless. |
| Crash without a managed stack trace | Run with `GTK_DEBUG=interactive` on a real display, or `G_DEBUG=fatal-warnings` to turn GTK warnings into failures locally. |
| Wrong screen size | `xvfb-run -s "-screen 0 1280x1024x24" -a dotnet test ...`. |

## Flakiness rules

* No `Thread.Sleep`, no `Task.Delay`, no wall-clock assertions.
* Do not assume a window manager is running (CI has none).
* Do not assert on window sizes after `Show()`; use `SetDefaultSize` and the
  `default-width`/`default-height` properties, which are deterministic.
* Do not depend on theme colours, fonts or DPI.

## API surface guard

`Core/ApiSurfaceTests` reflects over every wrapper assembly and compares the
public types and members against `PublicAPI/<Assembly>.txt`. Removing,
renaming or changing a public signature fails the suite.

If the change is intentional, regenerate the baselines and review the diff:

```sh
UPDATE_API_BASELINE=1 xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests \
  --filter "FullyQualifiedName~ApiSurfaceTests"

git diff Source/Tests/GtkSharp.Tests/PublicAPI
```

## Performance and memory

Performance and leak checks use the `Category=Performance` trait and are
excluded from the default `Test` target so the functional suite stays fast:

```sh
xvfb-run -a dotnet cake build.cake --BuildTarget=TestPerformance
```

They cover managed allocations per operation, CPU per operation, working-set
growth over create/dispose cycles, signal churn and wrapper leaks (the internal
`GLib.Object` registry is used because toggle refs keep undisposed widgets
alive forever). Thresholds and the Pi baseline are documented in
[perf/README.md](../perf/README.md).

Detailed benchmarks live in `Source/Tests/GtkSharp.Benchmarks`:

```sh
xvfb-run -a dotnet run -c Release --project Source/Tests/GtkSharp.Benchmarks
```

The CI `performance` job is advisory for now; promote it to a blocking gate
after three stable runs.

## Coverage

Coverage is collected with coverlet and excludes the generated bindings under
`Generated/`, because what matters is the hand-written wrapper code.

```sh
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release \
  --settings Source/Tests/GtkSharp.Tests/coverlet.runsettings \
  --collect:"XPlat Code Coverage" --results-directory TestResults
```

The report is written to `TestResults/<guid>/coverage.cobertura.xml`. The CI
publishes it as an artifact. There is no hard threshold yet: keep the
hand-written coverage from going down.

## CI

| Job | What it proves |
|:--|:--|
| `build` | The libraries, workload, templates and packages build on .NET 10. |
| `test` | The full suite passes under Xvfb on x64. |
| `arm64-smoke` | The workload builds, installs and runs a real GTK app on ARM64. |
| `publish` | Packages are pushed to GitHub Packages on `develop`/`master` only. |

A pull request is not ready to merge while `build`, `test` or `arm64-smoke`
is red.
