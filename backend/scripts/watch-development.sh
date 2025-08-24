#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP_PROJECT="$PROJECT_ROOT/src/Application/Application.csproj"

export ASPNETCORE_URLS=http://+:7777
export ASPNETCORE_ENVIRONMENT=Development

dotnet watch --project "$APP_PROJECT" run