# LovEat API — production Docker image (multi-stage build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LovEat.API.csproj ./
RUN dotnet restore LovEat.API.csproj

COPY . .
RUN dotnet publish LovEat.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl install pannunga FIRST (root ah irukumbodhu — apt-get ku root permission venum)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Appuram non-root user ku switch pannunga
RUN adduser --disabled-password --gecos '' loveat && chown -R loveat /app
USER loveat

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "LovEat.API.dll"]