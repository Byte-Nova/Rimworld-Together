using GameClient.Dialogs;
using GameClient.Misc;
using TCPNetwork.Packets;
using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using Verse.Steam;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;
using static Shared.Files.Configs.Mods.ModsConfigFile;
using Shared.Files.Configs.Mods;
using Shared.Misc;

namespace GameClient.Managers
{
    public static class ModManager
    {
        [HandlesPacket(PacketHeader.ModManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ModConfigData data = Serializer.ConvertBytesToObject<ModConfigData>(bytes);

            switch (data._stepMode)
            {
                case ModConfigStepMode.Ask:
                    OpenModManagerMenu();
                    break;
            }
        }

        public static void OpenModManagerMenu(bool isFirstEdit = false)
        {
            Action toDo = delegate 
            {
                GameParameterManager.SendCurrentModConfigs(false);
                if (isFirstEdit) GameParameterManager.SetFirstTimeSetup();
            };

            List<string> modNames = new List<string>();
            foreach (ModConfig config in ModManagerH.GetRunningModList().ModConfigs) modNames.Add(config.FileName);

            string[] keys = modNames.ToArray();
            string[] values = new string[] { "Required", "Optional", "Forbidden" };

            RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager", "Manage mods for the server", 
                keys, values, null, toDo, true);

            RT_Dialog_Base.PushNewDialog(dialog);
        }

        public static void ReceiveModConfigs(ServerGlobalData data)
        {
            SessionHandler.CurrentModConfig = data._modConfigs;

            if (!SessionHandler.CurrentModConfig.IsEnforced) return;
            else
            {
                Printer.Warning("Receiving mod configs from server", LogImportanceMode.Verbose);
                Printer.Warning("Currently doing nothing with the configs", LogImportanceMode.Verbose);
            }
        }
    }

    public static class ModManagerH
    {
        public static ModsConfigFile GetRunningModList()
        {
            ModsConfigFile configFile = new ModsConfigFile();

            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            foreach (ModContentPack mod in runningMods)
            {
                ModConfig newConfig = new ModConfig();
                newConfig.FileName = mod.Name.Replace("steam_", "");

                configFile.ModConfigs.Add(newConfig);
            }

            return configFile;
        }

        public static void GetConflictingMods(LoginData data)
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                data._extraDetails.ToArray()));
        }

        public static ModsConfigFile SortModsIntoCategories(string[] modNames, int[] categoryIndexes)
        {
            ModsConfigFile configFile = new ModsConfigFile();

            for (int i = 0; i < modNames.Length; i++)
            {
                ModConfig newConfig = new ModConfig();
                newConfig.FileName = modNames[i].Replace("steam_", "");
                newConfig.Type = (ModType)categoryIndexes[i];

                configFile.ModConfigs.Add(newConfig);
            }

            return configFile;
        }
    }
}
