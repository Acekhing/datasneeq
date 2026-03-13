FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/DataSneeq.Api/DataSneeq.Api.csproj src/DataSneeq.Api/
COPY src/DataSneeq.Application/DataSneeq.Application.csproj src/DataSneeq.Application/
COPY src/DataSneeq.Domain/DataSneeq.Domain.csproj src/DataSneeq.Domain/
COPY src/DataSneeq.Infrastructure/DataSneeq.Infrastructure.csproj src/DataSneeq.Infrastructure/

RUN dotnet restore src/DataSneeq.Api/DataSneeq.Api.csproj

COPY . .
RUN dotnet publish src/DataSneeq.Api/DataSneeq.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5041
ENV ConnectionStrings__AppDb="Data Source=/app/data/datasneeq.db"

EXPOSE 5041

ENTRYPOINT ["dotnet", "DataSneeq.Api.dll"]
