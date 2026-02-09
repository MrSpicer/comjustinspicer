#!/usr/bin/env bash

# Starts dotnet watch with hot reload for development, plus Sass watchers
set -euo pipefail
cd "$(dirname "$0")/.."
export ASPNETCORE_ENVIRONMENT=Development

# Start Sass watchers in background
npx sass --watch Comjustinspicer.CMS/Views/Shared/Components/ContentZone/edit.scss:Comjustinspicer.CMS/wwwroot/css/content-zone-edit.css --no-source-map &
SASS_PID1=$!
npx sass --watch Comjustinspicer.Web/Views/Shared/site.scss:Comjustinspicer.Web/wwwroot/css/site.css --no-source-map &
SASS_PID2=$!

# Clean up Sass watchers on exit
trap "kill $SASS_PID1 $SASS_PID2 2>/dev/null" EXIT

# Use dotnet watch to run the project with hot reload for both C# and Razor files
dotnet watch run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
