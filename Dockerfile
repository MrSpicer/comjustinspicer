# syntax=docker/dockerfile:1

# ---------------- Build Stage ----------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install Node.js and npm (required for SCSS compilation via npx sass)
RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
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

# Create data directory and set permissions
# The aspnet image includes a pre-configured 'app' user for non-root execution
RUN mkdir -p /data && chown -R app:app /app /data

USER app

ENTRYPOINT ["dotnet", "Comjustinspicer.Web.dll"]
