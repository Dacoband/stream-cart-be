# ChatBot Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files first for better layer caching
COPY ["src/ChatBotService/ChatBoxService.Api/ChatBoxService.Api.csproj", "src/ChatBotService/ChatBoxService.Api/"]
COPY ["src/ChatBotService/ChatBoxService.Application/ChatBoxService.Application.csproj", "src/ChatBotService/ChatBoxService.Application/"]
COPY ["src/ChatBotService/ChatBoxService.Infrastructure/ChatBoxService.Infrastructure.csproj", "src/ChatBotService/ChatBoxService.Infrastructure/"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "src/Shared/Shared.Common/"]

# Restore dependencies
RUN dotnet restore "src/ChatBotService/ChatBoxService.Api/ChatBoxService.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/ChatBotService/ChatBoxService.Api"
RUN dotnet build "ChatBoxService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChatBoxService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Install curl for health checks (similar to other services)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Set environment variables consistent with other services
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ChatBoxService.Api.dll"]