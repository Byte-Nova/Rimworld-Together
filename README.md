# RimWorld Together
## A Community Driven Multiplayer Mod!
### Mod is currently a work in progress! Please report any broken stuff you find!

Welcome to the Github repository for "RimWorld Together"! In here you will find everything related to the project, great place for the tech savvies!

> [!NOTE]
> Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3005289691
> 
> Mod Guide: https://rimworldtogether.github.io/Guide/
> 
> Discord Link: https://discord.gg/NCsArSaqBW
> 
> Mod Incompatibility list: https://github.com/RimworldTogether/Rimworld-Together/blob/development/IncompatibilityList.md

## Server Prequisites:
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
git submodules init
```

This will clone our [Guide](https://github.com/RimworldTogether/Guide) and Deployments project. To pull the latest, please use:
```sh
git submodules update
```

## Contributing
Please read our [contribution guide](https://github.com/RimworldTogether/Rimworld-Together/blob/development/CONTRIBUTING.md) before contributing!

## Other Questions?
Please don't hesitate to create and issue on Github if you have any question/issue with the server. We are here for you!
