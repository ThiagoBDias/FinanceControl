FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NuGet.Config ./NuGet.Config
COPY src/ ./src/

RUN dotnet restore src/FinanceControl.API/FinanceControl.API.csproj
RUN dotnet publish src/FinanceControl.API/FinanceControl.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinanceControl.API.dll"]
