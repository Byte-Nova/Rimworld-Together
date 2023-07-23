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

# Copy the output dll to dll_destination
dll_output_path = "Source/Client/bin/Release/net472/GameClient.dll"  # Adjust the path based on your configuration
shared_dll_output_path = "Source/Client/bin/Release/net472/"  # Adjust the path based on your configuration
dll_destination_path = os.path.join(destination_dir, "Current/Assemblies/RimworldTogether.dll")
shared_dll_destination_path = os.path.join(destination_dir, "Current/Assemblies/")

# If the destination path doesn't exist, create it
os.makedirs(os.path.dirname(dll_destination_path), exist_ok=True)

# Copy and rename the dll file
shutil.copy2(dll_output_path, dll_destination_path)

def copy_all_files(source_dir, destination_dir):
    # Check if the destination directory exists and if not, create it
    if not os.path.exists(destination_dir):
        os.makedirs(destination_dir)

    for filename in os.listdir(source_dir):
        file_path = os.path.join(source_dir, filename)

        # Only copy files (not directories)
        if os.path.isfile(file_path):
            shutil.copy2(file_path, destination_dir)


copy_all_files(shared_dll_output_path, shared_dll_destination_path)

# shutil.copy2(shared_dll_output_path, shared_dll_destination_path)
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
