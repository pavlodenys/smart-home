FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SmartHome.Api/SmartHome.Api.csproj", "SmartHome.Api/"]
COPY ["SmartHome.Data/SmartHome.Data.csproj", "SmartHome.Data/"]
COPY ["SmartHome.Core/SmartHome.Core.csproj", "SmartHome.Core/"]
COPY ["SmartHome.Logic/SmartHome.Logic.csproj", "SmartHome.Logic/"]
RUN dotnet restore "SmartHome.Api/SmartHome.Api.csproj"
COPY . .
WORKDIR "/src/SmartHome.Api"
RUN dotnet publish "SmartHome.Api.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 1433
COPY --from=build /app .

ENTRYPOINT ["dotnet", "SmartHome.Api.dll"]