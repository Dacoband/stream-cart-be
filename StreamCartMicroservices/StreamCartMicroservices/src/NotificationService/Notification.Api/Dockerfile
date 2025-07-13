FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationService/NotificationService.Api/NotificationService.Api.csproj", "NotificationService/NotificationService.Api/"]
COPY ["NotificationService/NotificationService.Application/NotificationService.Application.csproj", "NotificationService/NotificationService.Application/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "NotificationService/NotificationService.Api/NotificationService.Api.csproj"
COPY . .
WORKDIR "/src/NotificationService/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]