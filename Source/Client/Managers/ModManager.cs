using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;
using TCPNetwork.Packets;
using RimWorld;
using Shared;
using Shared.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using Verse.Steam;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;

namespace GameClient.Managers
{
    public static class ModManager
    {
        [HandlesPacket(PacketHeader.ModManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ModConfigData data = Serializer.ConvertBytesToObject<ModConfigData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case ModConfigStepMode.Ask:
                    OpenModManagerMenu();
                    break;
            }
        }

        public static void OpenModManagerMenu(bool isFirstEdit = false)
        {
            Action toDo = delegate { AskForSyncConfigs(isFirstEdit); };
            string[] keys = ModManagerH.GetRunningModList().UnsortedMods;
            string[] values = new string[] { "Required", "Optional", "Forbidden" };

            RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager", "Manage mods for the server", 
                keys, values, null, toDo);

            RT_Dialog_Base.PushNewDialog(dialog);
        }

        public static void ReceiveMods(ServerGlobalData data)
        {
            SessionValues.ConfigFile = data._modConfigs;

            if (!SessionValues.ConfigFile.EnforcedConfigs) return;
            else
            {
                Printer.Warning("Receiving mod configs from server", LogImportanceMode.Verbose);

                for (int i = 0; i < SessionValues.ConfigFile.ModFileNames.Length; i++)
                {
                    string filePath = GenFilePaths.ConfigFolderPath + Path.DirectorySeparatorChar + SessionValues.ConfigFile.ModFileNames[i];
                    if (File.Exists(filePath)) File.Delete(filePath);
                    File.WriteAllText(filePath, SessionValues.ConfigFile.ModConfigs[i]);

                    Printer.Warning($"Loaded > {SessionValues.ConfigFile.ModFileNames[i]}", LogImportanceMode.Verbose);
                }
            }
        }

        private static void AskForSyncConfigs(bool isFirstEdit)
        {
            Action toDoYes = delegate 
            { 
                GameParameterManager.SendCurrentModConfigs(true);
                if (isFirstEdit) GameParameterManager.SetFirstTimeSetup();
            };

            Action toDoNo = delegate 
            { 
                GameParameterManager.SendCurrentModConfigs(false);
                if (isFirstEdit) GameParameterManager.SetFirstTimeSetup();
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo("Do you want to enforce the mod settings?", toDoYes, toDoNo));
        }
    }

    public static class ModManagerH
    {
        public static string[] GetAllModConfigs()
        {
            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            string[] existingModConfigs = Directory.GetFiles(GenFilePaths.ConfigFolderPath);

            List<string> configsToFetch = new List<string>();
            foreach (ModContentPack mod in runningMods)
            {
                try
                {
                    string toGet = $"Mod_{mod.ModMetaData.GetPublishedFileId()}";
                    string toFetch = existingModConfigs.FirstOrDefault(fetch => fetch.Contains(toGet));
                    if (toFetch.Contains(toGet)) configsToFetch.Add(toFetch);
                    else Printer.Warning($"Config file for {mod.Name} did not exist, skipping");
                }
                catch { continue; }
            }

            return configsToFetch.ToArray();
        }

        public static ModConfigFile GetRunningModList()
        {
            List<string> loadedMods = new List<string>();
            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            foreach (ModContentPack mod in runningMods) loadedMods.Add(mod.PackageId);
            loadedMods.Sort();

            ModConfigFile configFile = new ModConfigFile();
            configFile.UnsortedMods = loadedMods.ToArray();
            return configFile;
        }

        public static void GetConflictingMods(byte[] bytes)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(bytes);

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginData._extraDetails.ToArray()));
        }

        public static ModConfigFile SortModsIntoCategories(string[] modNames, int[] categoryIndexes)
        {
            ModConfigFile configFile = new ModConfigFile();
            List<string> requiredMods = new List<string>();
            List<string> optionalMods = new List<string>();
            List<string> forbiddenMods = new List<string>();
            List<ulong> steamIds = new List<ulong>();

            for (int i = 0; i < modNames.Length; i++)
            {
                switch ((ModType)categoryIndexes[i])
                {
                    case ModType.Required:
                        requiredMods.Add(modNames[i]);
                        break;

                    case ModType.Optional:
                        optionalMods.Add(modNames[i]);
                        break;

                    case ModType.Forbidden:
                        forbiddenMods.Add(modNames[i]);
                        break;
                }

                ModMetaData mod = ModLister.GetActiveModWithIdentifier(modNames[i]);
                if (mod.OnSteamWorkshop) 
                {
                    Printer.Warning($"Mod {mod.PackageId} was on steam!", LogImportanceMode.Verbose);
                    WorkshopItemHook hook = mod.GetWorkshopItemHook();
                    steamIds.Add(hook.PublishedFileId.m_PublishedFileId);
                    Printer.Warning($"{hook.PublishedFileId.m_PublishedFileId}", LogImportanceMode.Verbose);
                }

                else 
                {
                    Printer.Warning($"Mod {mod.PackageId} was not on steam!", LogImportanceMode.Verbose);
                    steamIds.Add(0);
                }
            }

            configFile.UnsortedMods = modNames;
            configFile.RequiredMods = requiredMods.ToArray();
            configFile.OptionalMods = optionalMods.ToArray();
            configFile.ForbiddenMods = forbiddenMods.ToArray();
            configFile.AllModIds = steamIds.ToArray();

            return configFile;
        }
    }
}
