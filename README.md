# RimWorld Together
### *A Community Driven Multiplayer Mod for Rimworld!*
[![Steam Workshop](https://img.shields.io/badge/Steam_Workshop-Subscribe-blue?logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3005289691) [![Discord Link](https://img.shields.io/badge/Discord-Join-blue?logo=discord)](https://discord.gg/yUF2ec8Vt8) [![Mod Guide](https://img.shields.io/badge/Guide-Read-blue?logo=mdbook)](https://rimworldtogether.github.io/Guide/) [![Mod Incompatibility List](https://img.shields.io/badge/Incompatibility_List-View-blue?logo=markdown)](https://github.com/RimworldTogether/Rimworld-Together/blob/development/IncompatibilityList.md)

> [!NOTE]
> The mod is currently a work in progress! Please report any broken stuff you find!

Welcome to the repository for `RimWorld Together`! Here you will find everything related to the project, a great place for the tech savvies!

## Server Prerequisites:
Thanks to the way the server is built, there aren't any heavy hardware related dependencies, meaning that your hosting machine will only need to have an excellent network bandwith and a bit of everything else. Really, modern e-toasters could run it.

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

## Deployments:
To access our documentation and deployment examples, please run the following command:
```sh
git submodule update --init --recursive
```
This will clone (or update) our [Guide](https://github.com/RimworldTogether/Guide) and Deployments project. You can also use the `--recurse-submodules` flag on your initial clone to automatically pull the documentation at the same time.

## Contributing
Please read our [contribution guide](https://github.com/RimworldTogether/Rimworld-Together/blob/development/.github/CONTRIBUTING.md) before contributing!

## Other Questions?
Please don't hesitate to create an issue on Github if you have any questions/issues with the mod. We are here for you! You can also contact us via our Discord server (see top of this page).
