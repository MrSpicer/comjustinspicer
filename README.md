# comjustinspicer

## Development - Hot Reload

You can use dotnet's hot reload to update C# and Razor views without restarting the server.

From the repository root run:

```
cd Comjustinspicer.Web
./Scripts/RunHotReload.sh
```

This runs `dotnet watch run` with `ASPNETCORE_ENVIRONMENT=Development`. Razor view changes are picked up in Development because the project enables runtime compilation in `Program.cs`.
