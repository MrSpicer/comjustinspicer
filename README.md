# Comjustinspicer

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

### Development - Hot Reload
```
./Scripts/HotReloadRun.sh
```

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
