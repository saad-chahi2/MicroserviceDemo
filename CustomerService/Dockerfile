# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./CustomerService/*.csproj ./CustomerService/
RUN dotnet restore ./CustomerService/CustomerService.csproj

COPY ./CustomerService ./CustomerService
RUN dotnet publish ./CustomerService/CustomerService.csproj -c Release -o /app/publish

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:5002
ENTRYPOINT ["dotnet", "CustomerService.dll"]
