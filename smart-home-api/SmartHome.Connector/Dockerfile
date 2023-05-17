FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-connector
WORKDIR /src
COPY ["SmartHome.Connector/SmartHome.Connector.csproj", "SmartHome.Connector/"]
COPY ["SmartHome.Data/SmartHome.Data.csproj", "SmartHome.Data/"]
COPY ["SmartHome.Logic/SmartHome.Logic.csproj", "SmartHome.Logic/"]
RUN dotnet restore "SmartHome.Connector/SmartHome.Connector.csproj"
# Copy everything
COPY . .
WORKDIR "/src/SmartHome.Connector"
# Build and publish a release
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-connector /app .
ENTRYPOINT ["dotnet", "SmartHome.Connector.dll"]