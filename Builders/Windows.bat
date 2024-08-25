@echo off

::Declaration of all variables
set BASEBUILDERPATH=""
set BASEBUILDPATH=""
set BUILDRESULTPATH=""
set RIMWORLDPATH=""
set MODFOLDERPATH=""

::Setting variables
set BASEBUILDERPATH=%CD%
set /p RIMWORLDPATH=<RimWorldFolderPath.txt
set MODFOLDERPATH="%RIMWORLDPATH%/Mods/3005289691"
echo "RimWorld path is set to %RIMWORLDPATH%"
echo "RimWorld mod path is set to %MODFOLDERPATH%"

::Setting the BUILDRESULTPATH variable
cd Result
cd 3005289691
cd Current
cd Assemblies

::Deleting old assemblies
del /S /Q %CD% *.*

set BUILDRESULTPATH=%CD%
cd %BASEBUILDERPATH%

::Go and build the client
cd ..
cd Source
cd Client
dotnet build GameClient.csproj --configuration Release

::#Go and copy files to the result folder
cd bin
cd Release
cd net472
xcopy /Q /Y "GameClient.dll" "%BUILDRESULTPATH%"
xcopy /Q /Y "Newtonsoft.Json.dll" "%BUILDRESULTPATH%"

::#Go and copy files to RimWorld folder
cd %BUILDRESULTPATH%
cd ..
cd ..
mkdir %MODFOLDERPATH%
rmdir /S /Q %MODFOLDERPATH%
mkdir %MODFOLDERPATH%
xcopy /S /Q %CD% %MODFOLDERPATH%

::Ask if boot
cd %RIMWORLDPATH%
echo Boot RimWorld? (Y/N)
choice /c YN
if %errorlevel%==1 goto yes
if %errorlevel%==2 goto no

:yes
start RimWorldWin64.exe
exit

:no
exit