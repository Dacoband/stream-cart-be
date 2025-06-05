FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và phục hồi dependencies
COPY ["AccountService/AccountService.Api/AccountService.Api.csproj", "AccountService/AccountService.Api/"]
COPY ["AccountService/AccountService.Application/AccountService.Application.csproj", "AccountService/AccountService.Application/"]
COPY ["AccountService/AccountService.Domain/AccountService.Domain.csproj", "AccountService/AccountService.Domain/"]
COPY ["AccountService/AccountService.Infrastructure/AccountService.Infrastructure.csproj", "AccountService/AccountService.Infrastructure/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]

RUN dotnet restore "AccountService/AccountService.Api/AccountService.Api.csproj"

# Copy toàn bộ code
COPY . .
WORKDIR "/src/AccountService/AccountService.Api"

# Build và publish
RUN dotnet build "AccountService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "AccountService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AccountService.Api.dll"]