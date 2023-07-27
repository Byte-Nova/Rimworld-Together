import os
import shutil
import subprocess
import time

import psutil

modSteamId = "3005289691"  # Replace with your actual Steam ID

source_dir = "ModData"
destination_dir = f"Build/{modSteamId}"
source_solution = "Source/Client/GameClient.csproj"
if os.path.exists(destination_dir):
    shutil.rmtree(destination_dir)
# Copy files from source_dir to destination_dir
shutil.copytree(source_dir, destination_dir)

result = subprocess.run(["dotnet", "build", source_solution, "--configuration", "Debug"],
                        stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
print(result.stdout)
assert "Build succeeded." in result.stdout, "Build did not succeed"
assert "Build FAILED." not in result.stdout, "Build failed"
# Copy and rename the dll file
dll_output_dir = "Source/Client/bin/Debug/net472/"
dll_destination_dir = os.path.join(destination_dir, "Current/Assemblies/")

# Define an array of DLL names
dll_names = ["GameClient.dll", "AsyncIO.dll", "NetMQ.dll"]

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
    "C:/Games/Rimworld - Copy",
    "C:/Games/Rimworld",
    "C:/Program Files (x86)/Steam/steamapps/common/RimWorld"
]
truePath = ""
# Copy mod to each path if it exists
for mod_path1 in mod_paths:
    mod_path = os.path.join(mod_path1, "Mods")
    if os.path.exists(mod_path):
        mod_specific_path = os.path.join(mod_path, modSteamId)

        # Clear the specific mod directory if it already exists
        if os.path.exists(mod_specific_path):
            truePath = mod_path1
            shutil.rmtree(mod_specific_path)

        # Copy mod to mod_path
        shutil.copytree(destination_dir, mod_specific_path)

norm_exe_path = os.path.normcase(os.path.realpath(os.path.join(truePath, "RimWorldWin64.exe")))

# Check if the process is already running
for proc in psutil.process_iter(['pid', 'name', 'exe']):
    # Check whether the process name matches
    if proc.info['exe'] and os.path.normcase(os.path.realpath(proc.info['exe'])) == norm_exe_path:
        proc.kill()  # If so, kill the process

userA = "name=A" # name and password are the same
userB = "name=B"
fastConnect = "fastConnect=true" # shows button
instantConnect = "instantConnect=true" # auto connects, ignores the above button
forceDevMode = "forceDevMode=true" # shows dev mode button
subprocess.Popen([norm_exe_path, userA, fastConnect, instantConnect, forceDevMode])
time.sleep(2)
subprocess.Popen([norm_exe_path, userB, fastConnect, instantConnect, forceDevMode])
