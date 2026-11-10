# End-to-end checks

These scripts validate the two ways of consuming GtkSharp:

| Script | What it proves |
|:--|:--|
| `nuget-consumer.sh` | A plain `net10.0` and `net8.0` application can reference the `GtkSharp` NuGet package and use it without the workload. |
| `workload-template.sh` | The `gtkapp` template installed by the workload produces a `net10.0-gtk` application that builds and runs. |

Both need a display for GTK, so run them under `xvfb-run` on headless hosts.

```sh
# 1. Build the packages
dotnet cake build.cake --BuildVersion=3.24.24.200

# 2. NuGet consumer
xvfb-run -a eng/e2e/nuget-consumer.sh 3.24.24.200

# 3. Workload template
dotnet cake build.cake --BuildTarget=InstallWorkload --BuildVersion=3.24.24.200
xvfb-run -a eng/e2e/workload-template.sh
```

The workload is installed into the SDK pointed to by `DOTNET_ROOT`.
