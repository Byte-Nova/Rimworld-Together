using Shared;

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
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Master.loadedRequiredMods.Contains(str.ToLower())) Master.loadedRequiredMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded required mods [{Master.loadedRequiredMods.Count()}]", Logger.LogMode.Warning);

            Master.loadedOptionalMods.Clear();
            string[] optionalModsToLoad = Directory.GetDirectories(Master.optionalModsPath);
            foreach (string modPath in optionalModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Master.loadedOptionalMods.Contains(str.ToLower())) Master.loadedOptionalMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded optional mods [{Master.loadedOptionalMods.Count()}]", Logger.LogMode.Warning);

            Master.loadedForbiddenMods.Clear();
            string[] forbiddenModsToLoad = Directory.GetDirectories(Master.forbiddenModsPath);
            foreach (string modPath in forbiddenModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Master.loadedForbiddenMods.Contains(str.ToLower())) Master.loadedForbiddenMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded forbidden mods [{Master.loadedForbiddenMods.Count()}]", Logger.LogMode.Warning);
        }

        public static bool CheckIfModConflict(ServerClient client, JoinDetailsJSON loginDetailsJSON)
        {
            List<string> conflictingMods = new List<string>();

            if (Master.loadedRequiredMods.Count() > 0)
            {
                foreach (string mod in Master.loadedRequiredMods)
                {
                    if (!loginDetailsJSON.runningMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Required] > {mod}");
                        continue;
                    }
                }

                foreach (string mod in loginDetailsJSON.runningMods)
                {
                    if (!Master.loadedRequiredMods.Contains(mod) && !Master.loadedOptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        continue;
                    }
                }
            }

            if (Master.loadedForbiddenMods.Count() > 0)
            {
                foreach (string mod in Master.loadedForbiddenMods)
                {
                    if (loginDetailsJSON.runningMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Forbidden] > {mod}");
                    }
                }
            }

            if (conflictingMods.Count == 0)
            {
                client.runningMods = loginDetailsJSON.runningMods;
                return false;
            }

            else
            {
                if (client.isAdmin)
                {
                    Logger.WriteToConsole($"[Mod bypass] > {client.username}", Logger.LogMode.Warning);
                    client.runningMods = loginDetailsJSON.runningMods;
                    return false;
                }

                else
                {
                    Logger.WriteToConsole($"[Mod Mismatch] > {client.username}", Logger.LogMode.Warning);
                    UserManager.SendLoginResponse(client, CommonEnumerators.LoginResponse.WrongMods, conflictingMods);
                    return true;
                }
            }
        }
    }
}
