using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class ModManager
    {
        [HandlesPacket(PacketHeader.ModManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            ModConfigData data = Serializer.ConvertBytesToObject<ModConfigData>(bytes);

            switch (data._stepMode)
            {
                case ModConfigStepMode.Send:
                    SaveModConfig(client, data._configFile);
                    break;
            }
        }

        private static void SaveModConfig(ServerClient client, ModConfigFile file)
        {
            if (Master.WorldValues != null && !client.UserFile.IsAdmin)
            {
                UserManager.BanPlayerFromName(client.UserFile.Uid);
                Printer.Warning($"Player {client.UserFile.Uid} tried to change mod config without being admin");
            }

            else
            {
                Master.ModConfig = file;
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

            if (Master.ModConfig.RequiredMods.Length > 0)
            {
                foreach (string mod in Master.ModConfig.RequiredMods)
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
                    else if (!Master.ModConfig.RequiredMods.Contains(mod) && !Master.ModConfig.OptionalMods.Contains(mod))
                    {
                        conflictingMods.Add($"[Disallowed] > {mod}");
                        conflictingNames.Add(mod);
                        continue;
                    }
                }
            }

            //Check for forbidden mods

            if (Master.ModConfig.ForbiddenMods.Length > 0)
            {
                foreach (string mod in Master.ModConfig.ForbiddenMods)
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
                client.UserFile.UpdateMods(clientMods);
                return false;
            }

            else
            {
                if (client.UserFile.IsAdmin)
                {
                    InformationDisplayer.DisplayModBypass(client.UserFile.Label);
                    client.UserFile.UpdateMods(clientMods);
                    return false;
                }

                else
                {
                    InformationDisplayer.DisplayModMismatch(client.UserFile.Label);
                    LoginManagerH.DenyConnectionWithReason(client, LoginResponse.WrongMods, conflictingMods);
                    return true;
                }
            }
        }
    }
}
