# Comjustinspicer

This software is in pre-alpha development. do not use it

## License
CC BY-SA 4.0

## Built With
* [dotnet 8.0](https://dotnet.microsoft.com)
* [ASP.Net Core MVC](https://dotnet.microsoft.com/en-us/apps/aspnet)
* [SQLite](https://sqlite.org/)
* [Serilog](https://serilog.net/)
* [NUnit](https://nunit.org/)
* [AutoMapper](https://automapper.io/)

### Dependencies
* [dotnet sdk](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
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
### Run container
```
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="DataSource=/data/app.db" \
  -e AdminUser__Email="${ADMIN_EMAIL}" \
  -e AdminUser__Password="${ADMIN_PASSWORD}" \
  -e CKEditor__LicenseKey="${CKEDITOR_LICENSE_KEY}" \
  -e AutoMapper__LicenseKey="${AUTOMAPPER_LICENSE_KEY}" \
  -v appdata:/data \
  comjustinspicer-web
  ```

## Secrets

### .Net
```
dotnet user-secrets init --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Email" "<you@example.com>" --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Password" "<ChangeThisStrongPassword!>" --project Comjustinspicer.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "DataSource=app.db;Cache=Shared" --project Comjustinspicer.Web
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
