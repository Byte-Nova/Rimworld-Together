# !/bin/bash

# Declaration of all variables
BASEBUILDERPATH=""
BASEBUILDPATH=""
BUILDRESULTPATH=""
BUILDCONTENTPATH=""
RIMWORLDPATH=""
MODFOLDERPATH=""
FILEWITHGAMEPATH="RimWorldFolderPath.txt"

CheckIfFileWithPathIsPresent()
{
    if [ -f "$FILEWITHGAMEPATH" ]; then
        echo "$FILEWITHGAMEPATH exists, continuing operation";
        echo;
    else
        echo Put/Your/Path/To/Your/Game/In/Here > $FILEWITHGAMEPATH;
        echo "$FILEWITHGAMEPATH was just created, make sure to add your RimWorld path to it before you continue";
        read;
    fi
}
export -f CheckIfFileWithPathIsPresent;

SetAllNeededVariables()
{
    BASEBUILDERPATH=$(pwd);
    RIMWORLDPATH=$(cat $FILEWITHGAMEPATH);
    MODFOLDERPATH="$RIMWORLDPATH/Mods/3005289691";
    echo "RimWorld path is set to $RIMWORLDPATH";
    echo "RimWorld mod path is set to $MODFOLDERPATH";
    echo "Base builder path is set to $BASEBUILDERPATH";
    echo;

    cd Result;
    cd 3005289691;
    BUILDCONTENTPATH=$(pwd);
    echo "Build result path is set to $BUILDRESULTPATH";

    cd Current;
    mkdir "Assemblies" >/dev/null;
    cd Assemblies;
    BUILDRESULTPATH=$(pwd);
    echo "Build result path is set to $BUILDRESULTPATH";
    echo;
}
export -f SetAllNeededVariables;

DeleteOldAssemblies()
{
    rm -rf $BUILDRESULTPATH/*
}
export -f DeleteOldAssemblies

BuildClient()
{
    cd $BASEBUILDERPATH
    cd ..
    cd Source
    cd Client
    dotnet build GameClient.csproj --configuration Release

    # Go and copy files to the result folder
    cd bin
    cd Release
    cd net472
    cp GameClient.dll $BUILDRESULTPATH
    cp Newtonsoft.Json.dll $BUILDRESULTPATH

    # Go and copy files to RimWorld folder
    rm -rf $MODFOLDERPATH
    cp -r $BUILDCONTENTPATH $MODFOLDERPATH
}
export -f BuildClient;

DisplayEndingOptions()
{
    echo "Please enter your choice:";
    echo;
    options=("Start RimWorld" "Multi-start RimWorld" "Exit")
    select opt in "${options[@]}"
    do
        case $opt in
            "Start RimWorld") StartGame; break;;
            "Multi-start RimWorld") MultiStartGame; break;;
            "Exit") exit; break;;
            *) echo "invalid option '$REPLY'";;
        esac
    done
}
export -f DisplayEndingOptions;

StartGame()
{
    echo;
    cd $RIMWORLDPATH;
    sh -c "./RimWorldLinux";
}
export -f StartGame;

MultiStartGame()
{
    echo;
    cd $RIMWORLDPATH
    sh -c "./RimWorldLinux & ./RimWorldLinux";
}
export -f MultiStartGame;

CheckIfFileWithPathIsPresent;
SetAllNeededVariables;
DeleteOldAssemblies;
BuildClient;
DisplayEndingOptions;