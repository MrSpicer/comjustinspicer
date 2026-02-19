# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install Node.js 20 for SCSS compilation (MSBuild targets use npx sass)
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# Copy project files first — restoring in a separate layer caches NuGet packages
COPY Comjustinspicer.CMS/Comjustinspicer.CMS.csproj Comjustinspicer.CMS/
COPY Comjustinspicer.Web/Comjustinspicer.Web.csproj Comjustinspicer.Web/

# Restore with NuGet cache mount (persists between builds on host)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Comjustinspicer.Web/Comjustinspicer.Web.csproj

# Copy source and publish
COPY Comjustinspicer.CMS/ Comjustinspicer.CMS/
COPY Comjustinspicer.Web/ Comjustinspicer.Web/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish Comjustinspicer.Web/Comjustinspicer.Web.csproj \
      -c Release -o /app/publish --no-restore

# Runtime stage — Alpine for smaller image size
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# ICU libraries for .NET globalization (dates, strings, etc.)
RUN apk add --no-cache icu-libs icu-data-full

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Comjustinspicer.Web.dll"]
