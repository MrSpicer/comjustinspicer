#!/bin/bash
# Don't fail if app.db doesn't exist
rm -f app.db &&
rm -rf Migrations/Blog &&
#dotnet ef migrations add InitialApplication -c ApplicationContext -o Migrations/Application &&
dotnet ef migrations add InitialBlog -c BlogContext -o Migrations/Blog &&
dotnet ef database update -c BlogContext
