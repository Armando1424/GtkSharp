# GTK 4 port — state of `Gtk4Develop` and where help is needed

Analysis of branch `Gtk4Develop` (`4dfa061`, 2026-09-30) in `GtkSharp/GtkSharp`,
done on 2026-11-14 against a Debian 13 / .NET 10.0.404 / GTK 4.18.6 environment.

Context: PR #471 ("Port GtkSharp to Gtk 4.22") was closed on 2026-09-29 and split
into phases. PR #473 — "Gtk 4 port (1/4): GIR → api.xml converter and vendored
.gir files" — is merged into this branch. So this is **phase 1 of 4**.

## What the branch contains

| Item | State |
|---|---|
| `Source/Tools/GirToGapi` | Present: reads `.gir` directly, replaces the Perl `gapi2xml.pl` |
| `Source/Gir/` | 16 vendored `.gir` files (Gtk-4.0, Gdk-4.0, Gsk-4.0, Graphene-1.0, Adw-1, Gio, GLib, GObject, Pango, PangoCairo, GdkPixbuf, GtkSource-5, JavaScriptCore-6.0, WebKit-6.0) |
| `RegenerateApi` Cake target | Present and wired: builds `Source/Tools/Tools.sln`, then calls `RegenerateApi()` per assembly |
| `dotnet cake --BuildTarget=Build` | **Succeeds**: 0 errors, 358 warnings, ~3m50s |
| `_GtkSharpNetVersion` | `8.0` (libraries still build `net8.0` + `netstandard2.0`) |
| `cake.tool` | `2.0.0` (net8.0), so a .NET 8 runtime is still required |

## What the branch does *not* contain yet

The generated API surface is still **GTK 3**. Evidence from the checked-in files:

| Symbol | `GtkSharp-api.xml` | Meaning |
|---|---|---|
| `GtkContainer` | 48 references | GTK 3 only (removed in GTK 4) |
| `gtk_widget_show` | 3 | GTK 3 (GTK 4 removed it) |
| `GtkFileDialog` | 0 | GTK 4 (4.10+) missing |
| `gtk_box_append` | 0 | GTK 4 missing |
| `GdkSurface` / `GdkPaintable` | 0 | GTK 4 missing (`GdkSharp-api.xml` still has `gdk_window_new`) |
| `GrapheneSharp` project | absent | Graphene only exists as `.gir` |

### Root cause

`RegenerateApi` is a no-op today, and the code says so explicitly:

`CakeScripts/GAssembly.cake`:

> *"Only consumed by the RegenerateApi target, never by a normal build: the
> api.xml it produces is checked in, so building stays hermetic and needs no Gtk
> installed. **No assembly lists any yet** — the Gtk 4 wiring arrives with the
> regenerated bindings."*

And `CakeScripts/Settings.cake` still describes the GTK 3 assembly graph:

- `GtkSharp` depends on `AtkSharp` (ATK does not exist in GTK 4),
- no `GrapheneSharp` entry (Gdk 4 and Gtk 4 both need Graphene types),
- no assembly sets `Gir` or `GirIncludes`, so nothing is regenerated.

So the converter is finished enough to be *run*, but nothing drives it yet.

## How to help — suggested order

### Phase 2a: wire the converter (small, one commit)

1. In `CakeScripts/Settings.cake`, declare the inputs per assembly, for example:

   ```csharp
   new GAssembly("GtkSharp")
   {
       Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GdkSharp", "GrapheneSharp" },
       Gir = new[] { "Source/Gir/Gtk-4.0.gir" },
       GirIncludes = new[] { "Source/Gir/Gdk-4.0.gir", "Source/Gir/Gsk-4.0.gir",
                             "Source/Gir/Graphene-1.0.gir", "Source/Gir/GObject-2.0.gir" },
       ExtraArgs = "--abi-cs-usings=Gtk,GLib"
   }
   ```

2. Add `GrapheneSharp` (`Source/Gir/Graphene-1.0.gir`) and decide whether it is a
   separate assembly or part of `GdkSharp`.
3. Drop `AtkSharp` from the list (or keep it only for the GTK 3 line).
4. Run the target and commit the regenerated `*-api.xml`:

   ```bash
   dotnet cake build.cake --BuildTarget=RegenerateApi
   ```

### Phase 2b: triage the metadata

Once `api.xml` is GTK 4, the `.metadata` files (e.g.
`Source/Libs/GtkSharp/GtkSharp.metadata`) will reference types and methods that
no longer exist. The GTK 4 removals to expect: `Gtk.Container`, `Gtk.Bin`,
`Gtk.Widget.Show`/`ShowAll`, `Gtk.Widget.OnDrawn`/`ExposeEvent`, `Gtk.StateSet`,
`Gtk.ComboBox` and `Gtk.Calendar` restructured, `Atk` gone. Additions:
`Gtk.Snapshot`, `Gtk.EventController*`, `Gtk.FileDialog`, `Gtk.ColumnView`,
`Gtk.ListView`.

### Phase 3: make it compile

Only after the regenerated API is in place is it worth fixing hand-written code
(`Container.cs`, `Widget.cs`, `GtkSharp.sln` projects). Expect the biggest work
in `GdkSharp` (surfaces replace windows) and in anything that used
`Gtk.Container` child packing.

### Phase 4: tests

The branch already added an end-to-end suite. Extending it to GTK 4 widgets
(and running it under Xvfb in CI) would make each phase verifiable.

## Reproduce

```bash
docker build -t gtksharp-gtk4-dev .   # Debian 13 + .NET 10 SDK + .NET 8 runtime + libgtk-4-dev
docker run --rm -it --user $(id -u):$(id -g) \
  -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp/dotnet -e NUGET_PACKAGES=/tmp/nuget \
  -v "$PWD":/work -w /work gtksharp-gtk4-dev bash

dotnet tool restore
dotnet cake build.cake --BuildTarget=Build            # 0 errors
dotnet cake build.cake --BuildTarget=RegenerateApi    # currently a no-op
```

## Questions for the maintainer

1. Is removing `AtkSharp` acceptable, or should it stay for the GTK 3 line?
2. `Graphene`: separate assembly or folded into `GdkSharp`?
3. Which GTK 4 minor is the target — 4.18 (Debian 13) or 4.22 (the PR title)?
4. Should `.NET 10` become the baseline for the GTK 4 line, or stay on 8.0?
5. Is the 4-phase split still the plan, and is phase 2 (converter wiring) wanted as
   a PR of its own?
