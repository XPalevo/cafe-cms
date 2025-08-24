#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_PROJECT="$PROJECT_ROOT/src/Application/Application.csproj"
OUTPUT_DIR="$PROJECT_ROOT/publisher"

rm -rf "$OUTPUT_DIR"

dotnet publish "$APP_PROJECT" -c Release -r linux-x64 -o "$OUTPUT_DIR"

PUBLISH_DIR="$PROJECT_ROOT/publisher"
ENTRY_DLL="$PUBLISH_DIR/Application.dll"

if [ ! -f "$ENTRY_DLL" ]; then
  echo "Entry DLL not found: $ENTRY_DLL"
  exit 1
fi

export ASPNETCORE_URLS=http://+:7777
export ASPNETCORE_ENVIRONMENT=Development

dotnet "$ENTRY_DLL"
