#!/bin/bash

#Declaration of all variables
BASEBUILDERPATH=""
BASEBUILDPATH=""
BUILDRESULTPATH=""
RIMWORLDPATH=""
MODFOLDERPATH=""

#Setting variables
BASEBUILDERPATH=$(pwd)
RIMWORLDPATH=$(cat RimWorldFolderPath.txt)
MODFOLDERPATH="$RIMWORLDPATH/Mods/3005289691"
echo "RimWorld path is set to $RIMWORLDPATH"
echo "RimWorld mod path is set to $MODFOLDERPATH"

#Setting the BUILDRESULTPATH variable
cd Result
cd 3005289691
cd Current
cd Assemblies
BUILDRESULTPATH=$(pwd)

#Deleting old assemblies
rm -rf $(pwd)/*

#Go and build the client
cd $BASEBUILDERPATH
cd ..
cd Source
cd Client
sudo dotnet build GameClient.csproj --configuration Release

#Go and copy files to the result folder
cd bin
cd Release
cd net472
cp GameClient.dll $BUILDRESULTPATH
cp Newtonsoft.Json.dll $BUILDRESULTPATH

#Go and copy files to RimWorld folder
cd $BUILDRESULTPATH
cd ..
cd ..
rm -rf $MODFOLDERPATH
cp -r $(pwd) $MODFOLDERPATH

#Ask if boot
cd $RIMWORLDPATH
while true; do
    read -p "Boot RimWorld? (Yy/Nn)" yn
    case $yn in
        [Yy]* ) "./RimWorldLinux"; break;;
        [Nn]* ) exit;;
        * ) echo "Please answer Yy or Nn.";;
    esac
done
