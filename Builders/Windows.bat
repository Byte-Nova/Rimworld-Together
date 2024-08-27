@echo off

::Declaration of all variables
set BASEBUILDERPATH=""
set BASEBUILDPATH=""
set BUILDRESULTPATH=""
set RIMWORLDPATH=""
set MODFOLDERPATH=""
set FILEWITHGAMEPATH="RimWorldFolderPath.txt"

::Check if txt file with path is present
if exist %FILEWITHGAMEPATH% GOTO FileExists
GOTO FileDoesntExist

:FileExists
echo %FILEWITHGAMEPATH% exists, continuing operation
echo.
GOTO Execution

:FileDoesntExist
echo Put/Your/Path/To/Your/Game/In/Here > %FILEWITHGAMEPATH%
echo %FILEWITHGAMEPATH% was just created, make sure to add your RimWorld path to it before you continue
pause
echo.
GOTO Execution

:Execution
::Setting variables
set BASEBUILDERPATH=%CD%
set /p RIMWORLDPATH=<%FILEWITHGAMEPATH%
set MODFOLDERPATH="%RIMWORLDPATH%/Mods/3005289691"
echo RimWorld path is set to %RIMWORLDPATH%
echo RimWorld mod path is set to %MODFOLDERPATH%
echo.

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
echo.
cd ..
cd Source
cd Client
dotnet build GameClient.csproj --configuration Release

::Go and copy files to the result folder
echo.
cd bin
cd Release
cd net472
xcopy /Q /Y "GameClient.dll" "%BUILDRESULTPATH%"
xcopy /Q /Y "Newtonsoft.Json.dll" "%BUILDRESULTPATH%"

::Go and copy files to RimWorld folder
echo.
cd %BUILDRESULTPATH%
cd ..
cd ..
mkdir %MODFOLDERPATH%
rmdir /S /Q %MODFOLDERPATH%
mkdir %MODFOLDERPATH%
xcopy /S /Q %CD% %MODFOLDERPATH%

::Ask if boot
echo.
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
