#!/usr/bin/env bash

###THIS WILL DELETE YOUR DATA ###

echo "Removing existing database and migrations..." &&
rm -f Comjustinspicer.Web/app.db &&
rm -rf Comjustinspicer.CMS/Migrations/* &&
echo "Creating new migrations.." &&
echo "ApplicationDbContext" &&
dotnet ef migrations add InitialIdentity -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ApplicationDbContext -o Migrations/Identity &&
echo "BlogContext" &&
dotnet ef migrations add InitialBlog -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c BlogContext -o Migrations/Blog &&
echo "ContentBlockContext" &&
dotnet ef migrations add InitialContentBlock -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ContentBlockContext -o Migrations/ContentBlock &&
echo "Applying migrations to new database..." &&
#todo correct script or don't apply migrations because they will apply at startup
# ./Scripts/ApplyMigrations.sh &&
echo "Database rebuilt successfully."