FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY nuget.config .
COPY src/NotificationService.Domain/NotificationService.Domain.csproj src/NotificationService.Domain/
COPY src/NotificationService.Application/NotificationService.Application.csproj src/NotificationService.Application/
COPY src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj src/NotificationService.Infrastructure/
COPY src/NotificationService.Host/NotificationService.Host.csproj src/NotificationService.Host/
RUN dotnet restore src/NotificationService.Host/NotificationService.Host.csproj
COPY src/ src/
RUN dotnet publish src/NotificationService.Host/NotificationService.Host.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Host.dll"]
