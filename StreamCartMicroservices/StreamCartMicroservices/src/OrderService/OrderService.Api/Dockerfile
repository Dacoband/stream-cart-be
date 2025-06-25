FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrderService/OrderService.Api/OrderService.Api.csproj", "OrderService/OrderService.Api/"]
COPY ["OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]

RUN dotnet restore "OrderService/OrderService.Api/OrderService.Api.csproj"

# Copy all code
COPY . .
WORKDIR "/src/OrderService/OrderService.Api"

# Build and publish
RUN dotnet build "OrderService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]