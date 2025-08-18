build-network:
	dotnet build Source/TCPNetwork/TCPNetwork.csproj --configuration Release /property:WarningLevel=0

build-shared:
	dotnet build Source/Shared/Shared.csproj --configuration Release /property:WarningLevel=0

build-server:
	dotnet build Source/Server/GameServer.csproj --configuration Release /property:WarningLevel=0

build-client:
	dotnet build Source/Client/GameClient.csproj --configuration Release /property:WarningLevel=0
