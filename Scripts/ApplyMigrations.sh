#!/usr/bin/env bash

# Applies pending EF Core migrations to the database. This occures on application startup, but this script can be used to apply migrations manually if needed.

cd ./Comjustinspicer.CMS/
echo "Applying ArticleContext migrations" &&
dotnet ef database update -c ArticleContext &&
echo "Applying ApplicationDbContext migrations" &&
dotnet ef database update -c ApplicationDbContext &&
echo "Applying ContentBlockContext migrations" &&
dotnet ef database update -c ContentBlockContext &&
echo "All migrations applied successfully."
dotnet ef database update -c ContentZoneContext &&
dotnet ef database update -c PageContext &&
dotnet ef database update -c ArticleListContext &&
cd ../