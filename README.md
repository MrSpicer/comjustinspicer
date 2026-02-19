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
* [PostgreSQL](https://www.postgresql.org/)
* [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (optional) - ```dotnet tool install --global dotnet-ef --version 10.0.0```
* [docker](https://docs.docker.com/desktop/setup/install/linux/) (optional)

## Setup

### Make Scripts Executable
```
chmod +x ./Scripts/*
```

### Setup Local Database ###
```
./Scripts/SetupLocalPostgresDBContainer.sh
```

this will create a postgres 18-alpine docker container

### Development - Hot Reload
```
./Scripts/HotReloadRun.sh
```

The watch system monitors source files and automatically rebuilds when you save changes.

### Run Tests
```
./Scripts/TestsRun.sh
```

## Docker

### Build Image
```
./Scripts/DockerBuild.sh
```

### Run with Docker Compose
`DockerRun.sh` reads the same `dotnet user-secrets` used for local development — no additional secrets setup is required. It rewrites `Host=localhost` to `Host=db` in the connection string automatically before calling `docker compose up`.
```
./Scripts/DockerRun.sh
```

## Secrets

### .Net
```
dotnet user-secrets init --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Email" "<you@example.com>" --project Comjustinspicer.Web
dotnet user-secrets set "AdminUser:Password" "<ChangeThisStrongPassword!>" --project Comjustinspicer.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=comjustinspicer;Username=comjustinspicer;Password=<password>" --project Comjustinspicer.Web
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
## Troubleshooting

### Fix dotnet watch inotify Limits

If you encounter "The configured user limit on the number of inotify instances has been reached" error when running hot reload:

```bash
echo "fs.inotify.max_user_instances=8192" | sudo tee /etc/sysctl.d/99-inotify.conf
sudo sysctl -p /etc/sysctl.d/99-inotify.conf
```

This is a one-time setup per machine.