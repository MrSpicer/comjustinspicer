#!/usr/bin/env bash

###THIS WILL DELETE YOUR DATA ###

echo "Removing existing database and migrations..." &&
rm -f Comjustinspicer.Web/app.db &&
rm -rf Comjustinspicer.Web/Migrations/* &&
echo "Creating new migrations.." &&
echo "ApplicationDbContext" &&
dotnet ef migrations add InitialIdentity -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.Web/Comjustinspicer.Web.csproj -c ApplicationDbContext -o Migrations/Identity &&
echo "BlogContext" &&
dotnet ef migrations add InitialBlog -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.Web/Comjustinspicer.Web.csproj -c BlogContext -o Migrations/Blog &&
echo "ContentBlockContext" &&
dotnet ef migrations add InitialContentBlock -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.Web/Comjustinspicer.Web.csproj -c ContentBlockContext -o Migrations/ContentBlock &&
./Scripts/RunMigrations.sh &&
echo "Database rebuilt successfully."
