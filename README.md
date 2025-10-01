# Comjustinspicer

## Built With
* [dotnet 8.0](https://dotnet.microsoft.com)
* [ASP.Net Core MVC](https://dotnet.microsoft.com/en-us/apps/aspnet)
* [SQLite](https://sqlite.org/)
* [Serilog](https://serilog.net/)

### Dependencies
* [dotnet sdk](ttps://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* dotnet-ef - ```dotnet tool install --global dotnet-ef```

## Setup

### Make Scripts Executable
```
chmod +x Comjustinspicer.Web/Scripts/*
chmod +x Comjustinspicer.Tests/Scripts/*
```

### Create Database
```
cd Comjustinspicer.Web
./Scripts/RunMigrations.sh
```

## Development - Hot Reload
```
cd Comjustinspicer.Web
./Scripts/RunHotReload.sh
```

## Run Tests
```
cd Comjustinspicer.Tests
./Scripts/RunTests.sh
```

## notes
### Default Admin Account
a default Admin account is configured in Comjustinspicer.Web/appsettings.json
The account will be created on first run
you MUST change this before deploying to production

