# syntax=docker/dockerfile:1
# Build : docker build -f src/OrderService/Dockerfile -t saadchahi/orderservice:dev .
# (le contexte est la RACINE du repo, pour accéder à Directory.Build.props / Directory.Packages.props)

# ---------- Étape 1 : build (image SDK, lourde, jetée à la fin) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) D'abord uniquement les fichiers qui décrivent les dépendances.
#    Tant qu'ils ne changent pas, Docker réutilise le cache de la couche "restore" (build beaucoup plus rapide).
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/OrderService/OrderService.csproj src/OrderService/
RUN dotnet restore src/OrderService/OrderService.csproj

# 2) Ensuite le code source, puis publication.
COPY src/OrderService/ src/OrderService/
RUN dotnet publish src/OrderService/OrderService.csproj \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------- Étape 2 : runtime (image ASP.NET seule, légère) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Port standard des images .NET 8+ ; chaque conteneur/pod a sa propre IP, donc pas de conflit entre services.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Ne jamais tourner en root : l'image fournit l'utilisateur "app" (UID dans $APP_UID).
USER $APP_UID

ENTRYPOINT ["dotnet", "OrderService.dll"]
