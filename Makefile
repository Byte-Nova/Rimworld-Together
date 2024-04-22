
build-server:
	dotnet build Source/Server/GameServer.csproj --configuration Release

build-client:
	dotnet build Source/Client/GameClient.csproj --configuration Release


build-container:
	buildah build -t rwt:latest .

run-container:
	mkdir -p Data
	podman run -it --rm -v $(pwd)/Data:/Data:Z -p 25555:25555 rwt:latest
