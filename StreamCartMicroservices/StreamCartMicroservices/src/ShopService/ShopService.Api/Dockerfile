FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["ApiGateway/ApiGateway/ApiGateway.csproj", "ApiGateway/ApiGateway/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]

RUN dotnet restore "ApiGateway/ApiGateway/ApiGateway.csproj"

# Copy toàn bộ source code
COPY . .
WORKDIR "/src/ApiGateway/ApiGateway"

# Build và publish
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# Add curl here:
RUN apt-get update && apt-get install -y curl
COPY --from=publish /app/publish .
# Copy config files into the final image (relative to build context)
COPY ["ApiGateway/ApiGateway/ocelot.json", "./ocelot.json"]
COPY ["ApiGateway/ApiGateway/ocelot.Development.json", "./ocelot.Development.json"] 
COPY ["ApiGateway/ApiGateway/appsettings.json", "./appsettings.json"]
COPY ["ApiGateway/ApiGateway/appsettings.Development.json", "./appsettings.Development.json"]
ENTRYPOINT ["dotnet", "ApiGateway.dll"]