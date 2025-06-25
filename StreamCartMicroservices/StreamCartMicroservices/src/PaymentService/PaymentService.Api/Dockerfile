FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PaymentService/PaymentService.Api/PaymentService.Api.csproj", "PaymentService/PaymentService.Api/"]
COPY ["PaymentService/PaymentService.Application/PaymentService.Application.csproj", "PaymentService/PaymentService.Application/"]
COPY ["PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService/PaymentService.Infrastructure/"]
COPY ["PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "PaymentService/PaymentService.Domain/"]
COPY ["Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]
RUN dotnet restore "PaymentService/PaymentService.Api/PaymentService.Api.csproj"
COPY . .
WORKDIR "/src/PaymentService/PaymentService.Api"
RUN dotnet build "PaymentService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]