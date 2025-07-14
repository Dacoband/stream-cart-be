FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationService/Notification.Api/Notification.Api.csproj", "NotificationService/Notification.Api/"]
COPY ["NotificationService/Notification.Application/Notification.Application.csproj", "NotificationService/Notification.Application/"]
COPY ["NotificationService/Notification.Infrastructure/Notification.Infrastructure.csproj", "NotificationService/Notification.Infrastucture/"]
COPY ["NotificationService/Notification.Domain/Notification.Domain.csproj", "NotificationService/Notification.Domain/"]


COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "NotificationService/Notification.Api/Notification.Api.csproj"
COPY . .
WORKDIR "/src/NotificationService/Notification.Api"
RUN dotnet build "Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]