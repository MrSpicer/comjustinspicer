# Comjustinspicer

## setup

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

## Default Admin Account
a default Admin account is configured in Comjustinspicer.Web/appsettings.json
The account will be created on first run
you MUST change this before deploying to production

