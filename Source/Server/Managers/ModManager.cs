﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ModManager
    {
        public static void LoadMods()
        {
            Master.loadedRequiredMods.Clear();
            string[] requiredModsToLoad = Directory.GetDirectories(Master.requiredModsPath);
            foreach (string modPath in requiredModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (!Master.loadedRequiredMods.Contains(str.ToLower())) Master.loadedRequiredMods.Add(str.ToLower());
                    }
                }
                catch { ConsoleManager.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", LogMode.Error); }
            }

            ConsoleManager.WriteToConsole($"Loaded required mods [{Master.loadedRequiredMods.Count()}]", LogMode.Warning);

            Master.loadedOptionalMods.Clear();
            string[] optionalModsToLoad = Directory.GetDirectories(Master.optionalModsPath);
            foreach (string modPath in optionalModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (!Master.loadedRequiredMods.Contains(str.ToLower()))
                        {
                            if (!Master.loadedOptionalMods.Contains(str.ToLower())) Master.loadedOptionalMods.Add(str.ToLower());
                        }
                    }
                }
                catch { ConsoleManager.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", LogMode.Error); }
            }

            ConsoleManager.WriteToConsole($"Loaded optional mods [{Master.loadedOptionalMods.Count()}]", LogMode.Warning);

            Master.loadedForbiddenMods.Clear();
            string[] forbiddenModsToLoad = Directory.GetDirectories(Master.forbiddenModsPath);
            foreach (string modPath in forbiddenModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (!Master.loadedRequiredMods.Contains(str.ToLower()) && !Master.loadedOptionalMods.Contains(str.ToLower()))
                        {
                            if (!Master.loadedForbiddenMods.Contains(str.ToLower())) Master.loadedForbiddenMods.Add(str.ToLower());
                        }
                    }
                }
                catch { ConsoleManager.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", LogMode.Error); }
            }

            ConsoleManager.WriteToConsole($"Loaded forbidden mods [{Master.loadedForbiddenMods.Count()}]", LogMode.Warning);
        }

        public static bool CheckIfModConflict(ServerClient client, LoginData loginData)
        {
            List<string> conflictingMods = new List<string>();
            List<string> conflictingNames = new List<string>();

            if (Master.loadedRequiredMods.Count() > 0)
            {
                foreach (string mod in Master.loadedRequiredMods)
                {
                    if (!loginData.runningMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Required] > {mod}");
                        conflictingNames.Add(mod);
                        continue;
                    }
                }

                foreach (string mod in loginData.runningMods)
                {
                    if (conflictingNames.Contains(mod)) continue;
                    if (!Master.loadedRequiredMods.Contains(mod) && !Master.loadedOptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        conflictingNames.Add(mod);
                        continue;
                    }
                }
            }

            if (Master.loadedForbiddenMods.Count() > 0)
            {
                foreach (string mod in Master.loadedForbiddenMods)
                {
                    if (conflictingNames.Contains(mod)) continue;
                    if (loginData.runningMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Forbidden] > {mod}");
                        conflictingNames.Add(mod);
                    }
                }
            }

            if (conflictingMods.Count == 0)
            {
                client.runningMods = loginData.runningMods;
                return false;
            }

            else
            {
                if (client.isOperator)
                {
                    ConsoleManager.WriteToConsole($"[Mod bypass] > {client.username}", LogMode.Warning);
                    client.runningMods = loginData.runningMods;
                    return false;
                }

                else
                {
                    ConsoleManager.WriteToConsole($"[Mod Mismatch] > {client.username}", LogMode.Warning);
                    UserManager.SendLoginResponse(client, CommonEnumerators.LoginResponse.WrongMods, conflictingMods);
                    return true;
                }
            }
        }
    }
}
