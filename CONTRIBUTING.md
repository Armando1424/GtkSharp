# Contributing to GtkSharp

Thanks for taking the time to contribute. This document describes how the
project is built, how it is tested and what a pull request is expected to
contain.

## Branches

| Branch | Purpose |
|:--|:--|
| `develop` | Integration branch. All pull requests target this branch. |
| `master` | Stable branch, fast-forwarded from `develop` for releases. |
| `Gtk4Develop` | Experimental GTK4 work. Not the default target for fixes. |

Please do not open pull requests against `master`.

## Prerequisites

* .NET SDK **10.0.100** or newer. Run `dotnet --version` to check.
* GTK 3 development libraries (`libgtk-3-dev` on Debian/Ubuntu, `gtk+3` on
  macOS via Homebrew).
* On Linux without a desktop session: `xvfb` to run the UI tests.

## Build

```sh
dotnet tool restore
dotnet cake build.cake
```

The build generates the wrapper sources, compiles the libraries and packages
the NuGet packages and the workload. Output goes to `BuildOutput/`.

To work on a single library:

```sh
dotnet build Source/Libs/GtkSharp/GtkSharp.csproj -c Release
```

## Test

The test suite lives in `Source/Tests/GtkSharp.Tests` and runs on `net10.0`.

```sh
# Headless (recommended on CI and servers)
xvfb-run -a dotnet test Source/Tests/GtkSharp.Tests -c Release

# With a desktop session
dotnet test Source/Tests/GtkSharp.Tests -c Release

# Through the build script (builds first)
xvfb-run -a dotnet cake build.cake --BuildTarget=Test
```

GTK needs a display. When no display is available the GTK based tests are
**skipped** (not failed) and the GLib/Cairo/Pango tests still run. CI always
runs the full suite under `xvfb-run`.

See [docs/testing.md](docs/testing.md) for the test infrastructure, the
available helpers and the recommended patterns.

## Testing policy

Tests are part of the contribution, not an optional extra.

### When a test is required

| Change | Requirement |
|:--|:--|
| Bug fix in `Source/Libs/**` | A regression test that fails without the fix. |
| New or changed public API | At least one test covering the new behaviour. |
| Signal / event / marshalling change | A test that asserts the managed callback or value round-trip. |
| Workload, packaging or TFM change | The `arm64-smoke` CI job and/or a test that exercises the built workload. |
| Templates change | Manual verification is acceptable; describe the command you ran. |

### When a test is not required

Documentation, comments, formatting, CI-only changes and test-only refactors
do not need new tests.

If a behavioural change genuinely cannot be tested (for example, it depends on
a specific window manager or GPU), the pull request **must** include a section
explaining why:

```markdown
## Why no tests

<What cannot be automated, why, and the manual steps you followed to verify it.>
```

Pull requests without tests and without that justification are labelled
`no-tests` and may be sent back for more information. A maintainer can still
merge them, but the reason must be written down so the next person understands
the gap.

### What makes a good test here

* One behaviour per test. The name states the behaviour:
  `EntryChanged_FiresWhenTextIsSet`.
* Prefer public API and observable behaviour over implementation details.
* Widget relationships are compared through `Handle` (native pointer), not
  through reference identity: GtkSharp may return a different managed wrapper
  for the same native object.
* No `Thread.Sleep`, no timing assertions and no reliance on a window manager.
* Dispose every widget you create (`using var`).
* GTK tests must be in the `Gtk` collection (`[Collection(GtkCollection.Name)]`
  plus `[GtkFact]`) so they run serialized and are skipped automatically when
  there is no display.
* New test files go under `Source/Tests/GtkSharp.Tests` in the folder that
  matches the area (`Core`, `Widgets`, `Rendering`, `Interop`).

## Repository layout

```text
Source/
  Libs/           Wrapper libraries (GtkSharp, GLibSharp, CairoSharp, ...)
  Tools/          Code generators used by build.cake
  Workload/       .NET workload: SDK, reference pack, runtime pack, manifest
  Templates/      dotnet new templates
  Samples/        Sample application
  Tests/
    GtkSharp.Tests/    Unit and interop tests (net10.0)
    GtkSharp.SmokeTest/ End-to-end workload smoke test (net10.0-gtk)
build.cake        Build, package, install workload and test targets
docs/             Maintainer and contributor documentation
```

## Pull request checklist

* [ ] The change targets `develop`.
* [ ] `dotnet cake build.cake` succeeds.
* [ ] `xvfb-run -a dotnet cake build.cake --BuildTarget=Test` succeeds.
* [ ] New behaviour has tests, or the PR includes a `## Why no tests` section.
* [ ] Public API changes are reflected in the tests and, when relevant, in the
      README.
* [ ] The PR description explains **what** changed and **why**.
* [ ] No `BuildOutput/`, `bin/`, `obj/` or generated sources are committed.

## Reviewing a pull request

Maintainers should:

1. Confirm that the change is covered by a test that actually fails without it.
2. Run the test job and the ARM64 smoke job before merging.
3. Ask for the "why no tests" justification when tests are missing.
4. Keep `develop` green. Revert or fix forward; do not merge on a red build.
