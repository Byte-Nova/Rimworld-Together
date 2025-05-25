using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using Verse.Steam;
using static Shared.CommonEnumerators;

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
                    OpenModManagerMenu(false);
                    break;
            }
        }

        public static void OpenModManagerMenu(bool isFirstEdit)
        {
            Action toDo = delegate
            {
                AskForSyncConfigs(isFirstEdit);

                if (isFirstEdit) return;
                else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Mod configuration has been changed!" }));
            };

            string[] keys = ModManagerH.GetRunningModList().UnsortedMods;
            string[] values = new string[] { "Required", "Optional", "Forbidden" };
            RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager", "Manage mods for the server", keys, values, toDo);
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
            ModConfigData data = new ModConfigData();
            data._stepMode = ModConfigStepMode.Send;
            data._configFile = ModManagerH.SortModsIntoCategories(RT_Dialog_ListingWithTuple.DialogTupleListingResultString, 
                RT_Dialog_ListingWithTuple.DialogTupleListingResultInt);

            Action toDoYes = delegate 
            { 
                SendModConfigs(data);
                if (isFirstEdit) OnFirstEdit(); 
            };

            Action toDoNo = delegate
            {
                Network.Listener.EnqueuePacket(PacketHeader.ModManager, data);
                if (isFirstEdit) OnFirstEdit();
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_YesNo("Do you want to enforce the mod settings?",
                toDoYes, toDoNo));
        }

        public static void SendModConfigs(ModConfigData data)
        {
            List<string> modFileNames = new List<string>();
            List<string> modConfigs = new List<string>();
            foreach (string str in ModManagerH.GetAllModConfigs())
            {
                modFileNames.Add(Path.GetFileName(str));
                modConfigs.Add(File.ReadAllText(str));
            }
            data._configFile.ModFileNames = modFileNames.ToArray();
            data._configFile.ModConfigs = modConfigs.ToArray();
            data._configFile.EnforcedConfigs = true;

            Network.Listener.EnqueuePacket(PacketHeader.ModManager, data);
        }

        public static void OnFirstEdit()
        {
            Page toUse = new Page_SelectScenario();
            toUse.next = new Page_SelectStartingSite();
            RT_Dialog_Base.PushNewDialog(toUse);
        }
    }

    public static class ModManagerH
    {
        public static string[] GetAllModConfigs()
        {
            return Directory.GetFiles(GenFilePaths.ConfigFolderPath)
                .Where(fetch => Path.GetFileName(fetch).StartsWith("Mod_")).ToArray();
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

                var mod = ModLister.GetActiveModWithIdentifier(modNames[i]);

                if (mod.OnSteamWorkshop) 
                {
                    Printer.Warning($"Mod {mod.PackageId} was on steam!", LogImportanceMode.Verbose);
                    var hook = mod.GetWorkshopItemHook();
                    steamIds.Add(hook.PublishedFileId.m_PublishedFileId);
                    Printer.Warning($"{hook.PublishedFileId.m_PublishedFileId}");
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
