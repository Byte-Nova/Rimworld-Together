# Rimworld Together - SOURCE FILES
## A Community Driven Multiplayer Mod!
### Mod is currently a work in progress! Please report any broken stuff you find!

Welcome to the Github repository for "Rimworld Together"! In here you will find everything related to the server management part of the mod, great place for the tech savvies!

- Wiki: https://rimworld-together.fandom.com/wiki/Rimworld_Together_Wiki
- Discord: https://discord.gg/NCsArSaqBW
- Incompatibility list: https://docs.google.com/spreadsheets/d/14f4oJIV82SzqNK-Tyewr0OKxVRgge8xFasivACwRlsA/edit#gid=0

## Server Prequisites
The server runs utilizing the .NET 7.0 libraries, therefore you will need to have those said dependencies installed in your server machine. For quick access, you can download them from here: https://dotnet.microsoft.com/en-us/download/dotnet/7.0

**Either the Runtime version or the SDK will suffice, but any other won't make the server launch.**

Thanks to the way the server is built, there aren't any heavy hardware related dependencies, meaning that your hosting machine will only need to have an excellent network bandwith and a bit of everything else. Really, I'm sure modern e-toasters could run it.

## Server Installation
First, navigate towards the download section of this page and download the desired server version. We will always suggest the latest one as it usually comes with all the new bleeding edge features that old ones don't have: https://github.com/Nova-Atomic/Rimworld-Together/releases/latest

Then, place the server files somewhere where the server will be able to operate freely without any system/antivirus intervention (This is specially needed for linux users).

Execute the server once and close it again, all the needed files for configuration will have been generated. If they haven't, double check the server permissions are correctly set.

## Server Configuration
This is a really straight forward topic, really. The server will generate all the configurable files on first launch and will store them in the "CORE" folder.

Please check every one of the files that has been generated as all of them have important parameters for server functionality.

## Mod Management
On first launch, the server will also generate the "MODS" folder, inside of it will be another 3 folders, where different mods will go depending on how you want to enforce them. 
- Forbidden mods will kick the connecting player if its running them.
- Optional mods will allow a player to join even if it has them running or not.
- Required mods will kick the connecting player if its missing them.
  
If you are downloading the mods from Steam, you can use this tool to rename the folders to their actual mod names to make the modlist process easier: https://github.com/Nova-Atomic/Library

## Enabling Core & DLCs
To enable the use of Core and DLCs in the server, fetch the zip file called "Extras" from this repository and treat them as a folder of a normal mod and place them wherever you please in the mod folders.

## Port Forwarding & VPNs
The server, by default, uses the 25555 port through TCP protocol, you can change the port as you wish but remember that other than TCP it won't work. You can use VPN programs to go around the issue of port forwarding the same way you would do it with any other game.
  
To install mods, directly dump the mod folder (The one with the numbers in the title if grabing from Steam) inside whichever folder you choose.

## Other Questions?
Please don't hesitate to create and issue on Github if you have any question/issue with the server. We are here for you!

## Contribution
Make a fork of this repository and submit a pull request from said fork
