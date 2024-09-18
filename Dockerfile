FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY Source Source

# Restore as distinct layers
RUN dotnet build Source/Server/GameServer.csproj --configuration Release /property:WarningLevel=0

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App

COPY --from=build-env /App/Source/Server/bin/Release/net8.0 /App/Server

WORKDIR /Data

VOLUME /Data

EXPOSE 25555/tcp

ENTRYPOINT ["/App/Server/GameServer"]