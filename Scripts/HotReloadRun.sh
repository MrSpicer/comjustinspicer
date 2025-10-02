#!/usr/bin/env bash

# Starts dotnet watch with hot reload for development
set -euo pipefail
cd "$(dirname "$0")/.."
# Ensure Development environment and show messages
export ASPNETCORE_ENVIRONMENT=Development
# Use dotnet watch to run the project with hot reload for both C# and Razor files
dotnet watch run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
