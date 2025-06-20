FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShopService/ShopService.Api/ShopService.Api.csproj", "ShopService/ShopService.Api/"]
COPY ["ShopService/ShopService.Application/ShopService.Application.csproj", "ShopService/ShopService.Application/"]
COPY ["ShopService/ShopService.Domain/ShopService.Domain.csproj", "ShopService/ShopService.Domain/"]
COPY ["ShopService/ShopService.Infrastructure/ShopService.Infrastructure.csproj", "ShopService/ShopService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "ShopService/ShopService.Api/ShopService.Api.csproj"
COPY . .
WORKDIR "/src/ShopService/ShopService.Api"
RUN dotnet build "ShopService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShopService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopService.Api.dll"]