#!/usr/bin/env bash

cd ./Comjustinspicer.CMS/
echo "Applying ArticleContext migrations" &&
dotnet ef database update -c ArticleContext &&
echo "Applying ApplicationDbContext migrations" &&
dotnet ef database update -c ApplicationDbContext &&
echo "Applying ContentBlockContext migrations" &&
dotnet ef database update -c ContentBlockContext &&
echo "All migrations applied successfully."
cd ../