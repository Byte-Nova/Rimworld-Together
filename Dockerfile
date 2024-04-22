FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY Source Source

# Restore as distinct layers
RUN dotnet build Source/Server/GameServer.csproj --configuration Release

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App

COPY --from=build-env /App/Source/Server/bin/Release/net7.0 /App/Server

WORKDIR /Data

VOLUME /Data

EXPOSE 25555/tcp

ENTRYPOINT ["/App/Server/GameServer"]

