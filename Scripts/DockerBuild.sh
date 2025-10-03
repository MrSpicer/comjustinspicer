#!/usr/bin/env bash

# Rebuild image (ensure latest source in context)
docker build -t comjustinspicer-web . &&

echo "Built Docker image comjustinspicer-web"

# Remove old volume to force a clean DB (optional; will wipe data)
docker volume rm appdata || true