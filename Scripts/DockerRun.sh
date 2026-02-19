#!/usr/bin/env bash
# Reads secrets from dotnet user-secrets and runs docker compose up.

set -euo pipefail

PROJ="Comjustinspicer.Web/Comjustinspicer.Web.csproj"

SECRETS=$(dotnet user-secrets list --project "$PROJ" 2>/dev/null)

get_secret() {
    local key="$1"
    local line
    line=$(echo "$SECRETS" | grep "^${key} = " || true)
    if [[ -z "$line" ]]; then
        echo "ERROR: Missing required secret '${key}'" >&2
        echo "Add it with:" >&2
        echo "  dotnet user-secrets set \"${key}\" \"<value>\" --project Comjustinspicer.Web" >&2
        exit 1
    fi
    echo "${line#"${key} = "}"
}

# Read the existing local connection string and swap the host to the compose service name
LOCAL_CONN=$(get_secret "ConnectionStrings:DefaultConnection")
DB_CONNECTION=$(echo "$LOCAL_CONN" | sed 's/Host=[^;]*/Host=db/')

# Parse postgres credentials out of the connection string for the db service
POSTGRES_DB=$(echo "$DB_CONNECTION"       | grep -oP '(?<=Database=)[^;]+')
POSTGRES_USER=$(echo "$DB_CONNECTION"     | grep -oP '(?<=Username=)[^;]+')
POSTGRES_PASSWORD=$(echo "$DB_CONNECTION" | grep -oP '(?<=Password=)[^;]+')

ADMIN_EMAIL=$(get_secret "AdminUser:Email")
ADMIN_PASSWORD=$(get_secret "AdminUser:Password")
CKEDITOR_LICENSE_KEY=$(get_secret "CKEditor:LicenseKey")
AUTOMAPPER_LICENSE_KEY=$(get_secret "AutoMapper:LicenseKey")

export DB_CONNECTION
export POSTGRES_DB
export POSTGRES_USER
export POSTGRES_PASSWORD
export ADMIN_EMAIL
export ADMIN_PASSWORD
export CKEDITOR_LICENSE_KEY
export AUTOMAPPER_LICENSE_KEY

exec docker compose up
