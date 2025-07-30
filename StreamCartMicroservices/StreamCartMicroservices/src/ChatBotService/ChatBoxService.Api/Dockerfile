# ChatBot Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files và restore
COPY ["ChatBotService/ChatBoxService.Api/ChatBoxService.Api.csproj", "ChatBotService/ChatBoxService.Api/"]
COPY ["ChatBotService/ChatBoxService.Application/ChatBoxService.Application.csproj", "ChatBotService/ChatBoxService.Application/"]
COPY ["ChatBotService/ChatBoxService.Infrastructure/ChatBoxService.Infrastructure.csproj", "ChatBotService/ChatBoxService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]

RUN dotnet restore "ChatBotService/ChatBoxService.Api/ChatBoxService.Api.csproj"

# Copy toàn bộ source code
COPY . .

# Build application
WORKDIR "/src/ChatBotService/ChatBoxService.Api"
RUN dotnet build "ChatBoxService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ChatBoxService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Install curl cho health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ChatBoxService.Api.dll"]