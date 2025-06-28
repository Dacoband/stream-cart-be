FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CartService/CartService.Api/CartService.Api.csproj", "CartService/CartService.Api/"]
COPY ["CartService/CartService.Application/CartService.Application.csproj", "CartService/CartService.Application/"]
COPY ["CartService/CartService.Domain/CartService.Domain.csproj", "CartService/CartService.Domain/"]
COPY ["CartService/CartService.Infrastructure/CartService.Infrastructure.csproj", "CartService/CartService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "CartService/CartService.Api/CartService.Api.csproj"
COPY . .
WORKDIR "/src/CartService/CartService.Api"
RUN dotnet build "CartService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CartService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CartService.Api.dll"]