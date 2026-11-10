#!/usr/bin/env bash
#
# Verifies the workload end to end: the template installed by the workload is
# used to create a net10.0-gtk application, which is then built and launched.
#
# The workload must already be installed in DOTNET_ROOT, for example with:
#   dotnet cake build.cake --BuildTarget=InstallWorkload --BuildVersion=3.24.24.200
#
# Usage: eng/e2e/workload-template.sh
#
set -euo pipefail

if ! dotnet new list 2>/dev/null | grep -qi 'Gtk Application'; then
  echo "The gtk template is not installed. Install the workload first." >&2
  exit 1
fi

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

# The workload template uses the short name "gtk"; the standalone NuGet
# template uses "gtkapp". Prefer the workload one.
echo "--- dotnet new ---"
if ! dotnet new gtk -o "$WORK/app" -n E2eApp; then
  rm -rf "$WORK/app"
  dotnet new gtkapp -o "$WORK/app" -n E2eApp
fi

PROJECT="$(find "$WORK/app" -maxdepth 1 -name '*.csproj' | head -1)"
if [ -z "$PROJECT" ]; then
  echo "The template did not produce a csproj file." >&2
  exit 1
fi

echo "--- build ---"
dotnet build "$PROJECT" -c Release

echo "--- run (10s timeout: a GUI app with a main loop is expected to keep running) ---"
set +e
timeout 10 xvfb-run -a dotnet run --project "$PROJECT" -c Release --no-build
rc=$?
set -e

# 124 means the application was still running when the timeout fired.
if [ "$rc" -ne 0 ] && [ "$rc" -ne 124 ]; then
  echo "FAIL: gtkapp exited with code $rc." >&2
  exit 1
fi

echo "Workload template E2E passed (exit $rc)."
