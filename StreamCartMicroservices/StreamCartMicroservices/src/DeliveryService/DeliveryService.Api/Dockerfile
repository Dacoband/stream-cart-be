FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DeliveryService/DeliveryService.Api/DeliveryService.Api.csproj", "DeliveryService/DeliveryService.Api/"]
COPY ["DeliveryService/DeliveryService.Application/DeliveryService.Application.csproj", "DeliveryService/DeliveryService.Application/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "DeliveryService/DeliveryService.Api/DeliveryService.Api.csproj"
COPY . .
WORKDIR "/src/DeliveryService/DeliveryService.Api"
RUN dotnet build "DeliveryService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeliveryService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeliveryService.Api.dll"]