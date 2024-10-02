#!/bin/bash

#Declaration of all variables
BASEBUILDERPATH=""
BASEBUILDPATH=""
BUILDRESULTPATH=""
BUILDCONTENTPATH=""
RIMWORLDPATH=""
MODFOLDERPATH=""
FILEWITHGAMEPATH="RimWorldFolderPath.txt"

#Check if txt file with path is present
if [ -f "$FILEWITHGAMEPATH" ]; then
    echo "$FILEWITHGAMEPATH exists, continuing operation"
    echo
else
    echo Put/Your/Path/To/Your/Game/In/Here > $FILEWITHGAMEPATH
    echo "$FILEWITHGAMEPATH was just created, make sure to add your RimWorld path to it before you continue"
    read
fi

#Setting variables
BASEBUILDERPATH=$(pwd)
RIMWORLDPATH=$(cat $FILEWITHGAMEPATH)
MODFOLDERPATH="$RIMWORLDPATH/Mods/3005289691"
echo "RimWorld path is set to $RIMWORLDPATH"
echo "RimWorld mod path is set to $MODFOLDERPATH"
echo "Base builder path is set to $BASEBUILDERPATH"
echo

#Setting the more variables
cd Result
cd 3005289691
BUILDCONTENTPATH=$(pwd)
echo "Build result path is set to $BUILDRESULTPATH"

cd Current
mkdir "Assemblies"
cd Assemblies
BUILDRESULTPATH=$(pwd)
echo "Build result path is set to $BUILDRESULTPATH"
echo

#Deleting old assemblies
rm -rf $BUILDRESULTPATH/*

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
rm -rf $MODFOLDERPATH
cp -r $BUILDCONTENTPATH $MODFOLDERPATH

#Ask if boot
cd "$RIMWORLDPATH";
while true; do
    read -p "Boot RimWorld? (Yy/Nn)" yn
    case $yn in
        [Yy]* ) sh -c "./RimWorldLinux & ./RimWorldLinux"; break;;
        [Nn]* ) exit;;
        * ) echo "Please answer Yy or Nn.";;
    esac
done