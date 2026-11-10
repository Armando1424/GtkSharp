#!/usr/bin/env bash
#
# Verifies that the libraries built in BuildOutput/NugetPackages can be
# consumed as normal NuGet packages, without the GtkSharp workload.
#
# Usage: eng/e2e/nuget-consumer.sh [version]
#   version defaults to $GTKSHARP_VERSION or 3.24.24.200
#
set -euo pipefail

VERSION="${1:-${GTKSHARP_VERSION:-3.24.24.200}}"
PACKAGES_DIR="${PACKAGES_DIR:-BuildOutput/NugetPackages}"

if [ ! -d "$PACKAGES_DIR" ]; then
  echo "Package directory '$PACKAGES_DIR' does not exist. Run 'dotnet cake build.cake' first." >&2
  exit 1
fi

PACKAGES_DIR="$(cd "$PACKAGES_DIR" && pwd)"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

cat > "$WORK/NuGet.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$PACKAGES_DIR" />
  </packageSources>
</configuration>
EOF

cat > "$WORK/Program.cs" <<'CS'
using System;
using Gtk;

Gtk.Application.Init();

using var window = new Window("GtkSharp NuGet consumer");
using var button = Button.NewWithLabel("click");
var fired = false;
button.Clicked += (_, _) => fired = true;
window.Add(button);
button.Click();

if (!fired)
{
    Console.Error.WriteLine("FAIL: the Clicked event did not fire");
    return 1;
}

Console.WriteLine(
    $"OK: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription} " +
    $"consumed GtkSharp {typeof(Gtk.Window).Assembly.GetName().Version}");
return 0;
CS

test_consumer() {
  local tfm="$1"
  local dir="$WORK/$tfm"

  mkdir -p "$dir"
  cp "$WORK/NuGet.config" "$dir/NuGet.config"
  cp "$WORK/Program.cs" "$dir/Program.cs"

  cat > "$dir/consumer.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$tfm</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="GtkSharp" Version="$VERSION" />
  </ItemGroup>
</Project>
EOF

  echo "--- $tfm ---"
  dotnet build "$dir/consumer.csproj" -c Release
  xvfb-run -a dotnet run --project "$dir/consumer.csproj" -c Release --no-build
}

test_consumer net10.0

if dotnet --list-runtimes | grep -q 'Microsoft.NETCore.App 8\.'; then
  test_consumer net8.0
else
  echo "SKIP net8.0: the .NET 8 runtime is not installed."
fi

echo "NuGet consumer E2E passed for version $VERSION."
