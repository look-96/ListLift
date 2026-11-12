FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ListLift.Domain/ListLift.Domain.csproj src/ListLift.Domain/
COPY src/ListLift.Application/ListLift.Application.csproj src/ListLift.Application/
COPY src/ListLift.Infrastructure/ListLift.Infrastructure.csproj src/ListLift.Infrastructure/
COPY src/ListLift.PostgreSqlMigrations/ListLift.PostgreSqlMigrations.csproj src/ListLift.PostgreSqlMigrations/
COPY src/ListLift.Maintenance/ListLift.Maintenance.csproj src/ListLift.Maintenance/
COPY src/ListLift.API/ListLift.API.csproj src/ListLift.API/
COPY src/ListLift.Client/ListLift.Client.csproj src/ListLift.Client/
RUN dotnet restore src/ListLift.API/ListLift.API.csproj
RUN dotnet restore src/ListLift.Client/ListLift.Client.csproj
RUN dotnet restore src/ListLift.Maintenance/ListLift.Maintenance.csproj

COPY src/ src/
RUN dotnet publish src/ListLift.API/ListLift.API.csproj --configuration Release --no-restore --output /out/api
RUN dotnet publish src/ListLift.Client/ListLift.Client.csproj --configuration Release --no-restore --output /out/client
RUN dotnet publish src/ListLift.Maintenance/ListLift.Maintenance.csproj --configuration Release --no-restore --output /out/maintenance
RUN sed -i 's#https://localhost:7155/#/#' /out/client/wwwroot/appsettings.json \
    && sed -i "s#connect-src 'self' https://localhost:7155#connect-src 'self'#" /out/client/wwwroot/index.html \
    && mkdir -p /out/api/wwwroot \
    && cp -a /out/client/wwwroot/. /out/api/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /out/api/ ./
COPY --from=build /out/maintenance/ ./maintenance/
USER $APP_UID
ENTRYPOINT ["dotnet", "ListLift.API.dll"]
