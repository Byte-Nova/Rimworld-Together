# RimWorld Together
### *A Community Driven Multiplayer Mod for Rimworld!*
[![Steam Workshop](https://img.shields.io/badge/Steam_Workshop-Subscribe-blue?logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3005289691) [![Discord Link](https://img.shields.io/badge/Discord-Join-blue?logo=discord)](https://discord.gg/yUF2ec8Vt8) [![Mod Wiki](https://img.shields.io/badge/Wiki-Read-blue?logo=mdbook)](https://github.com/Byte-Nova/Rimworld-Together/wiki) [![Mod Incompatibility List](https://img.shields.io/badge/Incompatibility_List-View-blue?logo=markdown)](https://github.com/RimworldTogether/Rimworld-Together/blob/development/IncompatibilityList.md) [![Donate](https://img.shields.io/badge/Donate-Go-blue?logo=kofi)](https://ko-fi.com/rimworldtogether)

> [!NOTE]
> The mod is currently a work in progress! Please report any broken stuff you find!

Welcome to the repository for `RimWorld Together`! Here you will find everything related to the project, a great place for the tech savvies!

## Server Prerequisites:
Thanks to the way the server is built, there aren't any heavy hardware related dependencies, meaning that your hosting machine will only need to have an excellent network bandwith and a bit of everything else. Really, modern e-toasters could run it.

## Server Installation:
Download the latest server files at [here](https://github.com/Byte-Nova/Rimworld-Together/releases/latest). Then extract somewhere in your system and execute the executable file.
>[!WARNING]
> When extracting the server files somewhere, be mindful on where this location might be, avoid installing it at privileged locations (where admin permissions are needed) to prevent your antivirus/system kicking off.

## Container-based Deployment:
We provide helper scripts to build and run the official RimWorld-Together Container with [buildah](https://github.com/containers/buildah) and [podman](https://github.com/containers/podman)

**Build:**
```sh
make build-container
```

**Run:**
```sh
make run-container
```

## Contributing
Interested? Please read our [contribution guide](https://github.com/RimworldTogether/Rimworld-Together/blob/development/.github/CONTRIBUTING.md) before contributing!

## Other Questions?
Don't hesitate to create an issue on Github if you have any issues with the mod. We are here for you! You can also contact us via our [Discord server](https://discord.gg/yUF2ec8Vt8).
