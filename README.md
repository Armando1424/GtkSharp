# GtkSharp

GtkSharp is a C# wrapper for Gtk and its related components. The component list includes the following libraries: glib, gio, cairo, pango, atk, gdk. This is a fork of https://github.com/mono/gtk-sharp and is maintained completely separately from that project.

Differences can be seen with the following table:

|               | Target framework             | Target Gtk Version                                 | Extra notes                   |
|:--------------|:-----------------------------|:---------------------------------------------------|:------------------------------|
| GtkSharp      | .NET Standard 2.0 and .NET 10 | Gtk 3.22                                           | Does not need glue libraries. |
| mono/gtksharp | .NET Framework 4.5           | Gtk 2 (also Gtk 3.0 but never officially released) |                               |

* [Building from source](#building-from-source)
* [Testing](#testing)
* [Using the library](#using-the-library)
* [Workload](#workload)
* [Contributing](#contributing)
* [License](#license)

## Building from source

Requirements:

* .NET SDK **10.0.100** or newer.
* Gtk 3 development libraries (`libgtk-3-dev` on Debian/Ubuntu, `gtk+3` on macOS).
* `xvfb` when building or testing on a Linux machine without a desktop session.

To build the repository:

```sh
git clone https://github.com/GtkSharp/GtkSharp.git
cd GtkSharp
dotnet tool restore
dotnet cake build.cake
```

The build generates the wrapper sources, compiles the libraries and produces
the NuGet packages and the workload under `BuildOutput/`.

A breakdown of how the source is structured:

| Folder | Contents |
|:--|:--|
| [Source/Libs](Source/Libs) | The wrapper libraries: `GtkSharp`, `GLibSharp`, `GioSharp`, `GdkSharp`, `CairoSharp`, `PangoSharp`, `AtkSharp`, `GtkSourceSharp`, `WebkitGtkSharp`. |
| [Source/Tools](Source/Tools) | Code generators used during the build (`GapiCodegen`, `GapiFixup`). |
| [Source/Workload](Source/Workload) | The .NET SDK workload: SDK pack, reference pack, runtime pack and manifest. |
| [Source/Templates](Source/Templates) | `dotnet new` templates (`gtkapp`). |
| [Source/Samples](Source/Samples) | Sample application used for manual testing. |
| [Source/Tests](Source/Tests) | Automated tests and the workload smoke test. |
| [docs](docs) | Maintainer and contributor documentation. |
| [build.cake](build.cake) / [CakeScripts](CakeScripts) | Build, package, workload install and test targets. |

The libraries multi-target `net10.0` and `netstandard2.0`. The `netstandard2.0`
target keeps the NuGet packages usable from older target frameworks; the
`net10.0` target is what the workload ships.

## Testing

The test suite lives in [Source/Tests/GtkSharp.Tests](Source/Tests/GtkSharp.Tests)
and runs on `net10.0`.

```sh
# Headless (recommended on CI and servers)
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release

# With a desktop session
dotnet test Source/Tests/GtkSharp.Tests -c Release

# Through the build script
xvfb-run -a dotnet cake build.cake --BuildTarget=Test
```

GTK needs a display. When no display is available the GTK based tests are
skipped and the GLib/Cairo/Pango tests still run.

Tests are part of the contribution process. Every behavioural change is
expected to include a test, and pull requests without tests must explain why.
See [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/testing.md](docs/testing.md).

## Using the library

On macOS, you'll need to manually install Gtk, see [Installing Gtk on Mac](https://github.com/GtkSharp/GtkSharp/wiki/Installing-Gtk-on-Mac) wiki page for more details on how to do it.

Available NuGet packages:

* [GtkSharp](https://www.nuget.org/packages/GtkSharp/)
* [GdkSharp](https://www.nuget.org/packages/GdkSharp/)
* [GioSharp](https://www.nuget.org/packages/GioSharp/)
* [GLibSharp](https://www.nuget.org/packages/GLibSharp/)
* [AtkSharp](https://www.nuget.org/packages/AtkSharp/)
* [PangoSharp](https://www.nuget.org/packages/PangoSharp/)
* [CairoSharp](https://www.nuget.org/packages/CairoSharp/)
* [GtkSourceSharp](https://www.nuget.org/packages/GtkSourceSharp/)

To create a new gtk app project, simply use `dotnet new` templating engine:

* install: `dotnet new --install GtkSharp.Template.CSharp`
* uninstall: `dotnet new --uninstall GtkSharp.Template.CSharp`
* generate project: `dotnet new gtkapp`

## Workload

The repository also produces a `gtk` .NET workload, which enables the
`net10.0-gtk` target framework. Install it from the packages built in
`BuildOutput/NugetPackages` with the build script:

```sh
dotnet cake build.cake --BuildTarget=InstallWorkload --BuildVersion=3.24.24.1
```

The workload is installed into the SDK pointed to by `DOTNET_ROOT`. Supported
workload bands are listed in [build.cake](build.cake); the build adds the
current SDK feature band automatically when it is missing, so the install
target works with any .NET 10 SDK.

## Installing from GitHub Packages

The fork publishes stable builds to GitHub Packages. Add the feed to your
`NuGet.config` (authentication is required even for public packages) and
reference the version you want:

```xml
<PackageReference Include="GtkSharp" Version="3.24.24.200" />
```

See [docs/releasing.md](docs/releasing.md) for the feed URL, credentials and the
workload installation flow.

## Contributing

Pull requests target `develop`. Read [CONTRIBUTING.md](CONTRIBUTING.md) for the
build/test commands, the testing policy and the review checklist.

## License

GtkSharp and its related components are licensed under [LGPL v2.0 license](LICENSE), while [Samples](Source/Samples) are licenced under [The Unlicense](Source/Samples/LICENSE).
