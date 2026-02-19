# Sets up a PostgreSQL Docker container for local development
$ErrorActionPreference = 'Stop'
Set-Location "$PSScriptRoot/.."

$ContainerName = "comjustinspicer-db"
$VolumeName = "comjustinspicer-pgdata"

# Prompt for configuration
$DbName = Read-Host "Database name [comjustinspicer]"
if (-not $DbName) { $DbName = "comjustinspicer" }

$DbUser = Read-Host "Database user [comjustinspicer]"
if (-not $DbUser) { $DbUser = "comjustinspicer" }

$DbPass = Read-Host "Database password [postgres]"
if (-not $DbPass) { $DbPass = "postgres" }

$DbPort = Read-Host "Host port [5432]"
if (-not $DbPort) { $DbPort = "5432" }

# Check for Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not installed or not in PATH."
    exit 1
}

# Check for existing container
$existing = docker ps -a --format '{{.Names}}' | Where-Object { $_ -eq $ContainerName }
if ($existing) {
    Write-Host "Container '$ContainerName' already exists."
    Write-Warning "Removing the container will also delete the database volume. ALL DATA WILL BE LOST."
    $recreate = Read-Host "Remove container and all data? [y/N]"
    if ($recreate -match '^[Yy]$') {
        Write-Host "Removing existing container and volume..."
        docker rm -f $ContainerName
        docker volume rm $VolumeName --force
    } else {
        Write-Host "Aborting."
        exit 0
    }
}

# Start the container
Write-Host "Starting PostgreSQL container..."
docker run -d `
    --name $ContainerName `
    --restart unless-stopped `
    -e "POSTGRES_DB=$DbName" `
    -e "POSTGRES_USER=$DbUser" `
    -e "POSTGRES_PASSWORD=$DbPass" `
    -p "${DbPort}:5432" `
    -v "${VolumeName}:/var/lib/postgresql" `
    postgres:18-alpine

# Verify the container is running
Start-Sleep -Seconds 2
$running = docker ps --format '{{.Names}}' | Where-Object { $_ -eq $ContainerName }
if ($running) {
    Write-Host "Container '$ContainerName' is running."
} else {
    Write-Error "Container failed to start. Check 'docker logs $ContainerName' for details."
    exit 1
}

$ConnectionString = "Host=localhost;Port=$DbPort;Database=$DbName;Username=$DbUser;Password=$DbPass"

# Optionally save to dotnet user-secrets
$saveSecrets = Read-Host "Save connection string to dotnet user-secrets? [y/N]"
if ($saveSecrets -match '^[Yy]$') {
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $ConnectionString `
        --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
    Write-Host "Connection string saved to user-secrets."
}

Write-Host ""
Write-Host "Connection string:"
Write-Host "  $ConnectionString"
