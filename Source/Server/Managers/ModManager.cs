using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;

namespace RimworldTogether.GameServer.Managers
{
    public static class ModManager
    {
        public static void LoadMods()
        {
            Program.loadedRequiredMods.Clear();
            string[] requiredModsToLoad = Directory.GetDirectories(Program.requiredModsPath);
            foreach (string modPath in requiredModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Program.loadedRequiredMods.Contains(str.ToLower())) Program.loadedRequiredMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded required mods [{Program.loadedRequiredMods.Count()}]");

            Program.loadedOptionalMods.Clear();
            string[] optionalModsToLoad = Directory.GetDirectories(Program.optionalModsPath);
            foreach (string modPath in optionalModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Program.loadedOptionalMods.Contains(str.ToLower())) Program.loadedOptionalMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded optional mods [{Program.loadedOptionalMods.Count()}]");

            Program.loadedForbiddenMods.Clear();
            string[] forbiddenModsToLoad = Directory.GetDirectories(Program.forbiddenModsPath);
            foreach (string modPath in forbiddenModsToLoad)
            {
                try
                {
                    string aboutFile = Directory.GetFiles(modPath, "About.xml", SearchOption.AllDirectories)[0];
                    foreach (string str in XmlParser.ParseDataFromXML(aboutFile, "packageId"))
                    {
                        if (!Program.loadedForbiddenMods.Contains(str.ToLower())) Program.loadedForbiddenMods.Add(str.ToLower());
                    }
                }
                catch { Logger.WriteToConsole($"[Error] > Failed to load About.xml of mod at '{modPath}'", Logger.LogMode.Error); }
            }

            Logger.WriteToConsole($"Loaded forbidden mods [{Program.loadedForbiddenMods.Count()}]");
        }

        public static bool CheckIfModConflict(ServerClient client, LoginDetailsJSON loginDetailsJSON)
        {
            List<string> conflictingMods = new List<string>();

            if (Program.loadedRequiredMods.Count() > 0)
            {
                foreach (string mod in Program.loadedRequiredMods)
                {
                    if (!loginDetailsJSON.runningMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Required] > {mod}");
                        continue;
                    }
                }

                foreach (string mod in loginDetailsJSON.runningMods)
                {
                    if (!Program.loadedRequiredMods.Contains(mod) && !Program.loadedOptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        continue;
                    }
                }
            }

            if (Program.loadedForbiddenMods.Count() > 0)
            {
                foreach (string mod in Program.loadedForbiddenMods)
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
                if(client.isAdmin)
                {
                    Logger.WriteToConsole($"[Mod bypass] > {client.username}", Logger.LogMode.Warning);
                    client.runningMods = loginDetailsJSON.runningMods;
                    return false;
                }

                else
                {
                    UserManager_Joinings.SendLoginResponse(client, UserManager_Joinings.LoginResponse.WrongMods, conflictingMods);
                    return true;
                }
            }
        }
    }
}
