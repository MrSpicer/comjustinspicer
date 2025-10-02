#!/usr/bin/env bash

cd ./Comjustinspicer.Web/
echo "Applying BlogContext migrations" &&
dotnet ef database update -c BlogContext &&
echo "Applying ApplicationDbContext migrations" &&
dotnet ef database update -c ApplicationDbContext &&
echo "Applying ContentBlockContext migrations" &&
dotnet ef database update -c ContentBlockContext &&
echo "All migrations applied successfully."
cd ../