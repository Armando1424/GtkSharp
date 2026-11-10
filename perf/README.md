# Performance and memory

This folder documents how GtkSharp guards against performance regressions and
native memory leaks. The gate is the xUnit suite; the benchmarks are for
analysis.

## What is measured

| Signal | Test | Threshold (Pi 4 baseline) |
|:--|:--|:--|
| Managed allocations, Label create/dispose | `LabelCreateDispose_ManagedAllocationsAreBounded` | < 4 KB/op (measured ~1 KB) |
| CPU, Label create/dispose | `LabelCreateDispose_CpuIsBounded` | < 3 s / 2000 ops (measured ~0.18 s) |
| Working set, Label create/dispose | `LabelCreateDispose_DoesNotGrowWorkingSet` | < 32 MB / 5000 ops (measured ~1.2 MB) |
| Managed allocations, `GLib.Value` string | `GLibValueBoxing_ManagedAllocationsAreBounded` | < 512 B/op (measured ~32 B) |
| CPU, `GLib.Value` string | `GLibValueBoxing_CpuIsBounded` | < 2 s / 50k ops (measured ~0.06 s) |
| Managed allocations, `Label.Text` set | `LabelTextSet_ManagedAllocationsAreBounded` | < 1 KB/op (measured ~88 B) |
| CPU, `Label.Text` set | `LabelTextSet_CpuIsBounded` | < 2 s / 10k ops (measured ~0.06 s) |
| CPU, signal connect/disconnect | `SignalConnectDisconnect_CpuIsBounded` | < 2 s / 2000 pairs (measured ~0.05 s) |
| Working set, signal churn | `SignalConnectDisconnect_DoesNotGrowWorkingSet` | < 32 MB / 10k pairs |
| CPU, signal emit | `SignalEmit_CpuIsBounded` | < 5 s / 10k emits (measured ~0.28 s) |
| CPU, model iteration | `ModelIteration_CpuIsBounded` | < 5 s / 100k rows (measured ~?) |
| Wrapper leaks | `DisposedWidgets_ReleaseTheirNativeObjects` | < 200 registry entries / 2000 widgets |
| Leak counter sanity | `UndisposedWidgets_RemainInTheObjectRegistry` | detects 100 leaked wrappers |

Thresholds are intentionally generous: they catch leaks, accidental O(n²)
loops and unbounded allocation, not micro-variance. The values were measured on
a Raspberry Pi 4 and multiplied by 3–5x.

## Why the registry and not finalizers

GtkSharp keeps every live wrapper alive through a GLib toggle ref
(`GLib.ToggleRef`), so an undisposed widget never reaches its finalizer.
`GLib.Object.WarnOnFinalize` is therefore not a reliable leak signal for
widgets. The tests read the internal `GLib.Object.Objects` registry instead:

- created and disposed wrappers return the registry to its baseline;
- leaked wrappers stay in it.

## Running the gate

```sh
# Performance tests only
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release \
  --filter "Category=Performance"

# Through the build script
xvfb-run -a dotnet cake build.cake --BuildTarget=TestPerformance
```

The default `Test` target excludes `Category=Performance` so the functional
suite stays fast and deterministic. CI runs the performance job in advisory
mode; make it blocking once it has been stable for three consecutive runs.

## Benchmarks (BenchmarkDotNet)

```sh
xvfb-run -a dotnet run -c Release --project Source/Tests/GtkSharp.Benchmarks
xvfb-run -a dotnet run -c Release --project Source/Tests/GtkSharp.Benchmarks -- \
  --filter "*Label*"
```

The benchmarks use `[InProcess]` so BenchmarkDotNet does not rebuild the
libraries for every run. Use them to compare two revisions on the same machine;
do not compare absolute numbers across machines or architectures.

## Updating a threshold

1. Run the performance tests on the slowest supported machine.
2. Multiply the measured value by 3–5x.
3. Update the constant in `PerformanceTests.cs` and this table.
4. Run the gate three times in CI before tightening it.
