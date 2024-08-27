using Shared;
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
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true })[0];
                    foreach (string str in XmlParser.GetChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (Master.loadedRequiredMods.Contains(str))
                        {
                            continue;
                        }
                        Logger.Warning($"Loaded > '{modPath}'");
                        Master.loadedRequiredMods.Add(str);
                    }
                }
                catch { Logger.Error($"Failed to load About.xml of mod at '{modPath}'"); }
            }

            Master.loadedOptionalMods.Clear();
            string[] optionalModsToLoad = Directory.GetDirectories(Master.optionalModsPath);
            foreach (string modPath in optionalModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true })[0];
                    foreach (string str in XmlParser.GetChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (Master.loadedRequiredMods.Contains(str) || Master.loadedOptionalMods.Contains(str))
                        {
                            continue;
                        }
                        Logger.Warning($"Loaded > '{modPath}'");
                        Master.loadedOptionalMods.Add(str);
                    }
                }
                catch { Logger.Error($"Failed to load About.xml of mod at '{modPath}'"); }
            }

            Master.loadedForbiddenMods.Clear();
            string[] forbiddenModsToLoad = Directory.GetDirectories(Master.forbiddenModsPath);
            foreach (string modPath in forbiddenModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true })[0];
                    foreach (string str in XmlParser.GetChildContentFromParent(aboutFile, "packageId", "ModMetaData"))
                    {
                        if (Master.loadedRequiredMods.Contains(str) || Master.loadedOptionalMods.Contains(str) || Master.loadedForbiddenMods.Contains(str))
                        {
                            continue;
                        }
                        Logger.Warning($"Loaded > '{modPath}'");
                        Master.loadedForbiddenMods.Add(str);
                    }
                }
                catch { Logger.Error($"Failed to load About.xml of mod at '{modPath}'"); }
            }
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
                    }
                }

                foreach (string mod in loginData.runningMods)
                {
                    if (conflictingNames.Contains(mod)) continue;
                    if (!Master.loadedRequiredMods.Contains(mod) && !Master.loadedOptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        conflictingNames.Add(mod);
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
                client.userFile.UpdateMods(loginData.runningMods);
                return false;
            }

            else
            {
                if (client.userFile.IsAdmin)
                {
                    Logger.Warning($"[Mod bypass] > {client.userFile.Username}");
                    client.userFile.UpdateMods(loginData.runningMods);
                    return false;
                }

                Logger.Warning($"[Mod Mismatch] > {client.userFile.Username}");
                UserManager.SendLoginResponse(client, LoginResponse.WrongMods, conflictingMods);
                return true;
            }
        }
    }
}
