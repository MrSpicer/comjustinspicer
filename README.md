# Comjustinspicer

## Built With
* [dotnet 8.0](https://dotnet.microsoft.com)
* [ASP.Net Core MVC](https://dotnet.microsoft.com/en-us/apps/aspnet)
* [SQLite](https://sqlite.org/)
* [Serilog](https://serilog.net/)

### Dependencies
* [dotnet sdk](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* dotnet-ef - ```dotnet tool install --global dotnet-ef```

## Setup

### Make Scripts Executable
```
chmod +x ./Scripts/*
```

## Development - Hot Reload
```
./Scripts/HotReloadRun.sh
```

## Run Tests
```
./Scripts/RunTests.sh
```

## Docker

### Build and run
```
/Scripts/DockerRun.sh
```

## Notes
### Default Admin Account
a default Admin account is configured in Comjustinspicer.Web/appsettings.json and docker-compose.yml
The account will be created on first run
you MUST change this before deploying to production


## License
CC BY-NC-SA 4.0