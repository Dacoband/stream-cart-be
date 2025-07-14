FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notification/Notification.Api/Notification.Api.csproj", "Notification/Notification.Api/"]
COPY ["Notification/Notification.Application/Notification.Application.csproj", "Notification/Notification.Application/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "Notification/Notification.Api/Notification.Api.csproj"
COPY . .
WORKDIR "/src/Notification/Notification.Api"
RUN dotnet build "Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]