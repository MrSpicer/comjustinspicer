# syntax=docker/dockerfile:1

# ---------------- Build Stage ----------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY comjustinspicer.sln ./
COPY Comjustinspicer.Web/Comjustinspicer.Web.csproj Comjustinspicer.Web/
COPY Comjustinspicer.Tests/Comjustinspicer.Tests.csproj Comjustinspicer.Tests/
COPY Comjustinspicer.CMS/Comjustinspicer.CMS.csproj Comjustinspicer.CMS/

RUN dotnet restore

# Copy the rest of the source
COPY . .

# Publish (self-contained trimming not used yet for simplicity)
RUN dotnet publish Comjustinspicer.Web/Comjustinspicer.Web.csproj -c Release -o /app/publish --no-restore

# ---------------- Runtime Stage ----------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create volume mount points (SQLite data & logs if needed)
VOLUME ["/data"]

# Environment
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

# Expose HTTP port
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish ./

# Use non-root user (create if not existing in base image)
# The aspnet image already has an app user (UID 64198 historically) but we'll create a generic one
RUN adduser --disabled-password --gecos "app user" appuser || true \
    && chown -R appuser:appuser /app \
    && mkdir -p /data && chown -R appuser:appuser /data

USER appuser

ENTRYPOINT ["dotnet", "Comjustinspicer.Web.dll"]
