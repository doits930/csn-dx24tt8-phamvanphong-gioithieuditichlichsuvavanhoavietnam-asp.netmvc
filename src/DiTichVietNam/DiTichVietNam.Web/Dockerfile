FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DiTichVietNam.Web/DiTichVietNam.Web.csproj", "DiTichVietNam.Web/"]
RUN dotnet restore "./DiTichVietNam.Web/DiTichVietNam.Web.csproj"
COPY . .
WORKDIR "/src/DiTichVietNam.Web"
RUN dotnet build "./DiTichVietNam.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DiTichVietNam.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN mkdir -p /app/storage /app/wwwroot/uploads /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /app/storage /app/wwwroot/uploads /home/app/.aspnet
USER $APP_UID
ENTRYPOINT ["dotnet", "DiTichVietNam.Web.dll"]