#!/usr/bin/env bash

# Sets up a PostgreSQL Docker container for local development
set -euo pipefail
cd "$(dirname "$0")/.."

CONTAINER_NAME="comjustinspicer-db"
VOLUME_NAME="comjustinspicer-pgdata"

# Prompt for configuration
read -p "Database name [comjustinspicer]: " DB_NAME
DB_NAME="${DB_NAME:-comjustinspicer}"

read -p "Database user [comjustinspicer]: " DB_USER
DB_USER="${DB_USER:-comjustinspicer}"

read -p "Database password [devpass]: " DB_PASS
DB_PASS="${DB_PASS:-devpass}"

read -p "Host port [5432]: " DB_PORT
DB_PORT="${DB_PORT:-5432}"

# Check for Docker
if ! command -v docker &>/dev/null; then
    echo "Error: Docker is not installed or not in PATH."
    exit 1
fi

# Check for existing container
if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "Container '${CONTAINER_NAME}' already exists."
    read -p "Remove and recreate it? [y/N]: " RECREATE
    if [[ "${RECREATE:-N}" =~ ^[Yy]$ ]]; then
        echo "Removing existing container..."
        docker rm -f "${CONTAINER_NAME}"
    else
        echo "Aborting."
        exit 0
    fi
fi

# Start the container
echo "Starting PostgreSQL container..."
docker run -d \
    --name "${CONTAINER_NAME}" \
    --restart unless-stopped \
    -e POSTGRES_DB="${DB_NAME}" \
    -e POSTGRES_USER="${DB_USER}" \
    -e POSTGRES_PASSWORD="${DB_PASS}" \
    -p "${DB_PORT}:5432" \
    -v "${VOLUME_NAME}:/var/lib/postgresql" \
    postgres:18

# Verify the container is running
sleep 2
if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "Container '${CONTAINER_NAME}' is running."
else
    echo "Error: Container failed to start. Check 'docker logs ${CONTAINER_NAME}' for details."
    exit 1
fi

CONNECTION_STRING="Host=localhost;Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}"

# Optionally save to dotnet user-secrets
read -p "Save connection string to dotnet user-secrets? [y/N]: " SAVE_SECRETS
if [[ "${SAVE_SECRETS:-N}" =~ ^[Yy]$ ]]; then
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "${CONNECTION_STRING}" \
        --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
    echo "Connection string saved to user-secrets."
fi

echo ""
echo "Connection string:"
echo "  ${CONNECTION_STRING}"
