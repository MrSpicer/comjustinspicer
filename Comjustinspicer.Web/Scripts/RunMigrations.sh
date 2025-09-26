#! /bin/bash
echo "Applying BlogContext migrations" &&
dotnet ef database update -c BlogContext &&
echo "Applying ApplicationDbContext migrations" &&
dotnet ef database update -c ApplicationDbContext &&
echo "All migrations applied successfully."