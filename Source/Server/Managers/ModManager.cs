using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class ModManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            ModConfigData data = Serializer.ConvertBytesToObject<ModConfigData>(packet.contents);

            switch (data._stepMode)
            {
                case ModConfigStepMode.Send:
                    SaveModConfig(client, data._configFile);
                    break;
            }
        }

        private static void SaveModConfig(ServerClient client, ModConfigFile file)
        {
            if (Master.worldValues != null && !client.userFile.IsAdmin)
            {
                UserManager.BanPlayerFromName(client.userFile.Uid);
                Printer.Warning($"Player {client.userFile.Uid} tried to change mod config without being admin");
            }

            else
            {
                Master.modConfig = file;
                Main_.SaveValueFile(ServerFileMode.Mods, true);
                InformationDisplayer.DisplaySetMods(client);
            }
        }

        public static bool CheckIfModConflict(ServerClient client, LoginData loginData)
        {
            List<string> conflictingMods = new List<string>();
            List<string> conflictingNames = new List<string>();
            string[] clientMods = loginData._runningMods.UnsortedMods;

            //Check for required mods

            if (Master.modConfig.RequiredMods.Length > 0)
            {
                foreach (string mod in Master.modConfig.RequiredMods)
                {
                    if (!clientMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Required] > {mod}");
                        conflictingNames.Add(mod);
                        continue;
                    }
                }

                //Check for optional mods

                foreach (string mod in clientMods)
                {
                    if (conflictingNames.Contains(mod)) continue;
                    else if (!Master.modConfig.RequiredMods.Contains(mod) && !Master.modConfig.OptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        conflictingNames.Add(mod);
                        continue;
                    }
                }
            }

            //Check for forbidden mods

            if (Master.modConfig.ForbiddenMods.Length > 0)
            {
                foreach (string mod in Master.modConfig.ForbiddenMods)
                {
                    if (conflictingNames.Contains(mod)) continue;
                    else if (clientMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Forbidden] > {mod}");
                        conflictingNames.Add(mod);
                    }
                }
            }

            //Check for final conflicting count

            if (conflictingMods.Count == 0)
            {
                client.userFile.UpdateMods(clientMods);
                return false;
            }

            else
            {
                if (client.userFile.IsAdmin)
                {
                    InformationDisplayer.DisplayModBypass(client.userFile.Label);
                    client.userFile.UpdateMods(clientMods);
                    return false;
                }

                else
                {
                    InformationDisplayer.DisplayModMismatch(client.userFile.Label);
                    LoginManagerH.SendLoginResponse(client, LoginResponse.WrongMods, conflictingMods);
                    return true;
                }
            }
        }
    }
}
