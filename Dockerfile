FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WeatherApp.Domain/*.csproj ./WeatherApp.Domain/
COPY WeatherApp.Repository/*.csproj ./WeatherApp.Repository/
COPY WeatherApp.Service/*.csproj ./WeatherApp.Service/
COPY WeatherApp.Web/*.csproj ./WeatherApp.Web/

RUN dotnet restore WeatherApp.Web/WeatherApp.Web.csproj

COPY . .

WORKDIR /src/WeatherApp.Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherApp.Web.dll"]
