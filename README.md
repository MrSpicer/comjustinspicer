# Comjustinspicer

This software is in pre-alpha development. do not use it

## License
CC BY-SA 4.0

## Built With
* [dotnet 10.0](https://dotnet.microsoft.com)
* [ASP.Net Core MVC](https://dotnet.microsoft.com/en-us/apps/aspnet)
* [PostgreSQL](https://www.postgresql.org/)
* [Serilog](https://serilog.net/)
* [NUnit](https://nunit.org/)
* [AutoMapper](https://automapper.io/)

### Dependencies
* [dotnet sdk](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* [PostgreSQL](https://www.postgresql.org/download/) (or use Docker Compose/Setup Script)
* dotnet-ef (optional) - ```dotnet tool install --global dotnet-ef```

## Setup

### Make Scripts Executable
```
chmod +x ./Scripts/*
```

### Linux Development Setup

#### Fix dotnet watch inotify Limits

If you encounter "The configured user limit on the number of inotify instances has been reached" error when running hot reload:

```bash
echo "fs.inotify.max_user_instances=8192" | sudo tee /etc/sysctl.d/99-inotify.conf
sudo sysctl -p /etc/sysctl.d/99-inotify.conf
```

This is a one-time setup per machine. The project includes `.dotnetwatch.json` which automatically excludes build artifacts and IDE files from file watching.

### Development - Hot Reload
```
./Scripts/SetupLocalPostgresDBContainer.sh [optional - run once]
./Scripts/HotReloadRun.sh
```

The watch system monitors source files (`.cs`, `.cshtml`, `.csproj`, `.json`) and automatically rebuilds when you save changes.

### Run Tests
```
./Scripts/TestsRun.sh
```

## Docker

### Build Image
```
/Scripts/DockerBuild.sh
```
### Run with Docker Compose
```
docker compose up
```
### Run container standalone
```
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=comjustinspicer;Username=comjustinspicer;Password=devpass" \
  -e AdminUser__Email="${ADMIN_EMAIL}" \
  -e AdminUser__Password="${ADMIN_PASSWORD}" \
  -e CKEditor__LicenseKey="${CKEDITOR_LICENSE_KEY}" \
  -e AutoMapper__LicenseKey="${AUTOMAPPER_LICENSE_KEY}" \
  comjustinspicer-web
  ```

## Secrets

### .Net
```
dotnet user-secrets init --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Email" "<you@example.com>" --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Password" "<ChangeThisStrongPassword!>" --project Comjustinspicer.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=severname;Port=5432;Database=comjustinspicer;Username=comjustinspicer;Password=devpass" --project Comjustinspicer.Web
dotnet user-secrets set "CKEditor:LicenseKey" "<your-dev-license>" --project Comjustinspicer.Web
dotnet user-secrets set "AutoMapper:LicenseKey" "<your-dev-license>" --project Comjustinspicer.Web
```

### GitHub
```
ADMIN_EMAIL
ADMIN_PASSWORD
CONNECTION_STRING
CKEDITOR_LICENSE_KEY
AUTOMAPPER_LICENSE_KEY
```
