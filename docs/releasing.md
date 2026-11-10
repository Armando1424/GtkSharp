# Releasing stable packages

## Versioning

| Build | Version |
|:--|:--|
| Push to `develop` or `master` | `3.24.24.<run-number>-develop` |
| Tag `v3.24.24.200` | `3.24.24.200` |
| `GTKSHARP_VERSION=3.24.24.200` | `3.24.24.200` |

The version comes from, in order:

1. the `GTKSHARP_VERSION` environment variable;
2. a `v*` tag (the `v` prefix is stripped);
3. the workflow run number with a `-develop` suffix for non-`master` branches.

The manifest package is generated for the current SDK feature band and for the
bands listed in `build.cake` (currently `10.0.100`–`10.0.400`).

## Publishing a stable release

From the `release.yml` workflow:

* **Tag push**: `git tag v3.24.24.200 && git push origin v3.24.24.200`.
* **Manual run**: Actions → Release → Run workflow → version `3.24.24.200`.

Both paths build, package and push to GitHub Packages:

```text
https://nuget.pkg.github.com/Armando1424/index.json
```

The workflow uses `GITHUB_TOKEN` with `packages: write`.

## Consuming the packages

GitHub Packages requires authentication, even for public repositories. Add the
feed to `NuGet.config`:

```xml
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/Armando1424/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="Armando1424" />
      <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

Then reference the package:

```xml
<PackageReference Include="GtkSharp" Version="3.24.24.200" />
```

## Installing the workload from the feed

The SDK does not discover a custom workload automatically. The supported flow
(same as the maui-linux CI) is:

1. download `GtkSharp.NET.Sdk.Gtk.Manifest-<band>.<version>.nupkg`;
2. unzip `data/*` into
   `$DOTNET_ROOT/sdk-manifests/<band>/gtksharp.net.sdk.gtk/`;
3. run `dotnet workload install gtk --skip-manifest-update`;
4. `dotnet new gtk` to create a `net10.0-gtk` application.

Example:

```sh
BAND=10.0.400
VERSION=3.24.24.200
DOTNET_DIR=$DOTNET_ROOT

curl -fsSL \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -o manifest.nupkg \
  "https://nuget.pkg.github.com/Armando1424/download/gtksharp.net.sdk.gtk.manifest-$BAND/$VERSION"

mkdir -p "$DOTNET_DIR/sdk-manifests/$BAND/gtksharp.net.sdk.gtk"
unzip -j manifest.nupkg "data/*" -d "$DOTNET_DIR/sdk-manifests/$BAND/gtksharp.net.sdk.gtk"
rm manifest.nupkg

dotnet workload install gtk --skip-manifest-update
```

For a local build, use `dotnet cake build.cake --BuildTarget=InstallWorkload`,
which copies the packs and now also registers the templates with
`dotnet new install`.

## End-to-end validation

Before publishing, the CI runs:

| Job | What it proves |
|:--|:--|
| `consumers` | `net10.0` and `net8.0` apps consume the NuGet packages without the workload. |
| `workload-e2e` | The workload installs, the `gtk` template builds and the app runs. |
| `arm64-smoke` | The workload works on ARM64. |

Run them locally with:

```sh
dotnet cake build.cake --BuildVersion=3.24.24.200
xvfb-run -a eng/e2e/nuget-consumer.sh 3.24.24.200

dotnet cake build.cake --BuildTarget=InstallWorkload --BuildVersion=3.24.24.200
xvfb-run -a eng/e2e/workload-template.sh
```
