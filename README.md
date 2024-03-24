# Rimworld Together - SOURCE FILES
## A Community Driven Multiplayer Mod!
### Mod is currently a work in progress! Please report any broken stuff you find!

Welcome to the Github repository for "Rimworld Together"! In here you will find everything related to the server management part of the mod, great place for the tech savvies!

> [!NOTE]
> Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3005289691
>
> Wiki: https://rimworld-together.fandom.com/wiki/Rimworld_Together_Wiki
>
> Trello: https://trello.com/b/Y5J3IeSX/rimworld-together
> 
> Discord: https://discord.gg/NCsArSaqBW
>
> Incompatibility list: https://docs.google.com/spreadsheets/d/14f4oJIV82SzqNK-Tyewr0OKxVRgge8xFasivACwRlsA/edit#gid=0

> [!CAUTION]
> Make sure the client mod is loaded as high as possible to guarantee the mod working properly.

## Server Prequisites
Thanks to the way the server is built, there aren't any heavy hardware related dependencies, meaning that your hosting machine will only need to have an excellent network bandwith and a bit of everything else. Really, modern e-toasters could run it.

## Server Installation
First, navigate towards the download section of this page and download the desired server version. We will always suggest the latest one as it usually comes with all the new bleeding edge features that old ones don't have: https://github.com/Byte-Nova/Rimworld-Together/releases/latest

Then, just open the executable file for the first time to let it generate the configuration files, and close it to configure it manually, should it be needed.

> [!CAUTION]
> When placing the files of the server somewhere, be specially careful of the location to prevent any system/antivirus interventions. This is extra important in Linux operating systems

## Container-based Deployment

We provide helper scripts to build and run the official RimWorld-Together Container with [buildah](https://github.com/containers/buildah) and [podman](https://github.com/containers/podman)


**Build:**

```sh
make build-container
```

**Run:**

```sh
make run-container
```

## Server Configuration
The server will generate all the configurable files on first launch and will store them in the "CORE" folder.

Please check every one of the files that has been generated as all of them have important parameters for server functionality.

> [!TIP]
> For a quick and easy way to setup custom difficulty, please follow this guide: https://rimworld-together.fandom.com/wiki/Custom_difficulty

## Mod Management
On first launch, the server will generate the "MODS" folder, inside of it will be another 3 folders, where different mods will go depending on how you want to enforce them. 
- Forbidden mods will kick the connecting player if its running them.
- Optional mods will allow a player to join even if it has them running or not.
- Required mods will kick the connecting player if its missing them.

To install mods, directly dump the mod folder (The one with the numbers in the title if grabing from Steam) inside whichever folder you choose.
Furthermore, the files for Harmony, Hugslib and Rimworld Together can be found in the Extras, which can be found in this repository.

> [!TIP]
> If you are downloading the mods from Steam, you can use this tool to rename the folders to their actual mod names to make the modlist process easier: https://github.com/Byte-Nova/Library

## Enabling Core & DLCs
To enable the use of Core and DLCs in the server, fetch the zip file called "Extras" from this repository and treat them as a folder of a normal mod and place them wherever you please in the mod folders.

## Port Forwarding & VPNs
The server, by default, uses the 25555 port through TCP protocol, you can change the port as you wish but remember that other than TCP it won't work. You can use VPN programs to go around the issue of port forwarding the same way you would do it with any other game.

> [!TIP]
> For a quick and easy guide to port forwarding and VPNs, please follow this guide: https://rimworld-together.fandom.com/wiki/Going_public

## Other Questions?
Please don't hesitate to create and issue on Github if you have any question/issue with the server. We are here for you!
