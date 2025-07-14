FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LivestreamService/LivestreamService.Api/LivestreamService.Api.csproj", "LivestreamService/LivestreamService.Api/"]
COPY ["LivestreamService/Livestreamservice.Application/Livestreamservice.Application.csproj", "LivestreamService/Livestreamservice.Application/"]
COPY ["LivestreamService/Livestreamservice.Domain/Livestreamservice.Domain.csproj", "Livestreamservice/Livestreamservice.Domain/"]
COPY ["LivestreamService/LivestreamService.Infrastructure/LivestreamService.Infrastructure.csproj", "LivestreamService/LivestreamService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]

RUN dotnet restore "LivestreamService/LivestreamService.Api/LivestreamService.Api.csproj"

# Copy all code
COPY . .
WORKDIR "/src/LivestreamService/LivestreamService.Api"

# Build and publish
RUN dotnet build "LivestreamService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "LivestreamService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LivestreamService.Api.dll"]