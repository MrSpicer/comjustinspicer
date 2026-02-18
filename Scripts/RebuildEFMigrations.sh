#!/usr/bin/env bash

echo "Removing existing migrations..." &&
rm -rf Comjustinspicer.CMS/Migrations/* &&
echo "Creating new migrations.." &&
echo "ApplicationDbContext" &&
dotnet ef migrations add InitialIdentity -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ApplicationDbContext -o Migrations/Identity &&
echo "ArticleContext" &&
dotnet ef migrations add InitialArticle -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ArticleContext -o Migrations/Article &&
echo "ContentBlockContext" &&
dotnet ef migrations add InitialContentBlock -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ContentBlockContext -o Migrations/ContentBlock &&
echo "ContentZone" &&
dotnet ef migrations add InitialContentZone -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c ContentZoneContext -o Migrations/ContentZone &&
echo "PageContext" &&
dotnet ef migrations add InitialPage -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c PageContext -o Migrations/Page &&
echo "Database rebuilt successfully."
