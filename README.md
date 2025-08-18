# RimWorld Together
### A Community Driven Multiplayer Mod for Rimworld!
### Welcome to our repository! A great place for the tech savvies!
[![](https://img.shields.io/badge/Steam_Workshop-Subscribe-blue?logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3005289691) 
[![](https://img.shields.io/badge/Discord-Join-blue?logo=discord)](https://discord.gg/yUF2ec8Vt8) 
[![](https://img.shields.io/badge/Wiki-Read-blue?logo=mdbook)](https://github.com/Byte-Nova/Rimworld-Together/wiki) 
[![](https://img.shields.io/badge/Incompatibility_List-View-blue?logo=markdown)](https://github.com/RimWorld-Together/Incompatibilities)
[![](https://img.shields.io/badge/Expansion_List-View-blue?logo=markdown)](https://github.com/RimWorld-Together/Expansions)
[![](https://img.shields.io/badge/Donate-Go-blue?logo=kofi)](https://ko-fi.com/rimworldtogether)

> [!NOTE]
> The mod is currently a work in progress! Please report any broken stuff you find!

## Server Prerequisites:
Thanks to the way the server is built, there aren't any heavy hardware related dependencies, meaning that your hosting machine will only need to have an excellent network bandwith and a bit of everything else. Really, modern e-toasters could run it.

## Server Installation:
Download the latest server files at [here](https://github.com/Byte-Nova/Rimworld-Together/releases/latest). Then extract somewhere in your system and execute the executable file.
>[!WARNING]
> When extracting the server files somewhere, be mindful on where this location might be, avoid installing it at privileged locations (where admin permissions are needed) to prevent your antivirus/system kicking off.

## Container-based Deployment:
We provide an official container image hosted on the Github Container Repository and an example docker-compose file to run the server.

The following commands allow you to runs the server after creating a `RWTData` directory that will contain all the server's user files.

Make sure you have [Docker](https://www.docker.com/) (or [Podman](https://github.com/containers/podman)) installed.

### 1 - Create a Data directory
```sh
mkdir -p RWTData
```

### 2 - Run the server

**Run with Docker on Linux:**
```sh
docker run -it --rm -v './RWTData:/Data' -p 25555:25555 ghcr.io/rimworld-together/rimworld-together:latest
```
**Run with Docker on Windows:**
```sh
docker run -it --rm -v '.\RWTData:/Data' -p 25555:25555 ghcr.io/rimworld-together/rimworld-together:latest
```


## Contributing
Interested? Please read our [contribution guide](https://github.com/RimworldTogether/Rimworld-Together/blob/development/.github/CONTRIBUTING.md) before contributing!

## Other Questions?
Don't hesitate to create an issue on Github if you have any issues with the mod. We are here for you! You can also contact us via our [Discord server](https://discord.gg/yUF2ec8Vt8).
