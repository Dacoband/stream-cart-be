FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy csproj files and restore as distinct layers
COPY ["AccountService.Api.csproj", "AccountService.Api/"]
COPY ["../AccountService.Application/AccountService.Application.csproj", "AccountService.Application/"]
COPY ["../AccountService.Domain/AccountService.Domain.csproj", "AccountService.Domain/"]
COPY ["../AccountService.Infrastructure/AccountService.Infrastructure.csproj", "AccountService.Infrastructure/"]
RUN dotnet restore "AccountService.Api/AccountService.Api.csproj"

# Copy everything else and build
COPY . ./AccountService.Api/
WORKDIR "/src/AccountService.Api"
RUN dotnet build "AccountService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.Api.dll"]