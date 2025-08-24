# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Projeleri copy (restore optimizasyonu için ayrı ayrı)
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj src/Api/

RUN dotnet restore src/Api/Api.csproj

# Kodun tamamı
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Healthcheck için curl
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Container içi port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Prod env
ENV ASPNETCORE_ENVIRONMENT=Production

# MediatR lisansı (build arg → runtime env)
ARG MEDIATR_LICENSE_KEY
ENV MEDIATR_LICENSE_KEY=${MEDIATR_LICENSE_KEY}

# Healthcheck (container içinden /health)
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

# Yayın çıktısını kopyala
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
