# GtkSharp tests

* `GtkSharp.Tests` — unit, signal, marshalling and interop tests (`net10.0`).
* `GtkSharp.SmokeTest` — end-to-end test that uses the built workload
  (`net10.0-gtk`) and verifies that a real GTK app can be compiled and run.

```sh
# Unit and interop tests
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release

# Workload smoke test (requires the workload to be installed in DOTNET_ROOT)
xvfb-run -a dotnet run --project Source/Tests/GtkSharp.SmokeTest -c Release
```

Read [docs/testing.md](../../docs/testing.md) before adding tests.
