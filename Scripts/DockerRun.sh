#!/usr/bin/env bash

# Rebuild image (ensure latest source in context)
docker build -t comjustinspicer-web . &&

echo "Built Docker image comjustinspicer-web"

# Remove old volume to force a clean DB (optional; will wipe data)
#docker volume rm appdata || true

# Run container
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="DataSource=/data/app.db" \
  -e AdminUser__Email="admin@example.com" \
  -e AdminUser__Password="ChangeMe123!" \
  -v appdata:/data \
  comjustinspicer-web