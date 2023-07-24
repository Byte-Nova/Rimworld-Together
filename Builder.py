import os
import shutil
import subprocess

modSteamId = "3005289691"  # Replace with your actual Steam ID

source_dir = "ModData"
destination_dir = f"Build/{modSteamId}"
source_solution = "Source/Client/GameClient.csproj"
if os.path.exists(destination_dir):
    shutil.rmtree(destination_dir)
# Copy files from source_dir to destination_dir
shutil.copytree(source_dir, destination_dir)

# Build the C# project
subprocess.run(["dotnet", "build", source_solution, "--configuration", "Release"])

# Copy and rename the dll file
dll_output_dir = "Source/Client/bin/Release/net472/"
dll_destination_dir = os.path.join(destination_dir, "Current/Assemblies/")

# Define an array of DLL names
dll_names = ["GameClient.dll", "Shared.dll", "AsyncIO.dll", "NetMQ.dll"]

# If the destination path doesn't exist, create it
os.makedirs(dll_destination_dir, exist_ok=True)

# Iterate over DLL names
for dll_name in dll_names:
    # Create full paths for each DLL
    dll_output_path = os.path.join(dll_output_dir, dll_name)
    dll_destination_path = os.path.join(dll_destination_dir, dll_name)

    # Copy the DLL file
    shutil.copy2(dll_output_path, dll_destination_path)

mod_paths = [
    "C:/Games/Rimworld/Mods",
    "C:/Program Files (x86)/Steam/steamapps/common/RimWorld/Mods"
]

# Copy mod to each path if it exists
for mod_path in mod_paths:
    if os.path.exists(mod_path):
        mod_specific_path = os.path.join(mod_path, modSteamId)

        # Clear the specific mod directory if it already exists
        if os.path.exists(mod_specific_path):
            shutil.rmtree(mod_specific_path)

        # Copy mod to mod_path
        shutil.copytree(destination_dir, mod_specific_path)
