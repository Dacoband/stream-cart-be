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
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=publish /app/publish .
# Copy config files into the final image (relative to build context)
COPY ApiGateway/ApiGateway/ocelot.json .
COPY ApiGateway/ApiGateway/appsettings.json .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]