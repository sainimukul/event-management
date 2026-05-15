# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/EventManagement.Domain/EventManagement.Domain.csproj         src/EventManagement.Domain/
COPY src/EventManagement.Application/EventManagement.Application.csproj src/EventManagement.Application/
COPY src/EventManagement.Infrastructure/EventManagement.Infrastructure.csproj src/EventManagement.Infrastructure/
COPY src/EventManagement.Api/EventManagement.Api.csproj               src/EventManagement.Api/
RUN dotnet restore src/EventManagement.Api/EventManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/EventManagement.Api/EventManagement.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5050 \
    ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5050
USER app

ENTRYPOINT ["dotnet", "EventManagement.Api.dll"]
