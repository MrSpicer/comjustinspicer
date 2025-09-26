#!/bin/bash
# Don't fail if app.db doesn't exist
echo "Removing existing database and migrations..." &&
rm -f app.db &&
rm -rf Migrations/* &&
echo "Creating new migrations.." &&
dotnet ef migrations add InitialIdentity -s comjustinspicer.csproj -p comjustinspicer.csproj -c ApplicationDbContext -o Migrations/Identity &&
dotnet ef migrations add InitialBlog -c BlogContext -o Migrations/Blog &&
echo "Updating database..." &&
dotnet ef database update -c ApplicationDbContext &&
dotnet ef database update -c BlogContext &&
echo "Database rebuilt successfully."
