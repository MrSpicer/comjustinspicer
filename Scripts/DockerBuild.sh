#!/usr/bin/env bash

DOCKER_BUILDKIT=1 docker build -t comjustinspicer-web .

echo "Built Docker image comjustinspicer-web"
