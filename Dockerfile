# syntax=docker/dockerfile:1
#
# One image for the whole site: the React SPA is built with Node, copied
# into the API's wwwroot, and served by the same ASP.NET Core process -
# the same shape as the Plesk deployment.

# ---- 1. Frontend -----------------------------------------------------
FROM node:22-bookworm-slim AS web
WORKDIR /src/web
COPY apps/web/package.json apps/web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY apps/web/ ./
RUN npm run build

# ---- 2. Backend ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api
WORKDIR /src
# Restore first, from the project files only, so the NuGet layer is
# cached until a .csproj changes.
COPY apps/api/Portfolio.Domain/Portfolio.Domain.csproj                 Portfolio.Domain/
COPY apps/api/Portfolio.Application/Portfolio.Application.csproj       Portfolio.Application/
COPY apps/api/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj Portfolio.Infrastructure/
COPY apps/api/Portfolio.Api/Portfolio.Api.csproj                       Portfolio.Api/
RUN dotnet restore Portfolio.Api/Portfolio.Api.csproj
COPY apps/api/ ./
RUN dotnet publish Portfolio.Api/Portfolio.Api.csproj -c Release -o /app/publish --no-restore

# ---- 3. Runtime ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=api /app/publish ./
COPY --from=web /src/web/dist ./wwwroot/

# Run as the image's built-in non-root user; it only needs to write to
# the local uploads folder (used when Storage:Provider is Local).
RUN mkdir -p /app/wwwroot/uploads && chown -R "$APP_UID" /app/wwwroot/uploads
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
