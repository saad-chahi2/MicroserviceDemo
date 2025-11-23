FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./OrderService/*.csproj ./OrderService/
RUN dotnet restore ./OrderService/OrderService.csproj

COPY ./OrderService ./OrderService
RUN dotnet publish ./OrderService/OrderService.csproj -c Release -o /app/publish
		
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .   
ENV ASPNETCORE_URLS="http://0.0.0.0:5001"
ENTRYPOINT ["dotnet", "OrderService.dll"]