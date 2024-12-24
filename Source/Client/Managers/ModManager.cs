using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class ModManager
    {
        public static void ParsePacket(Packet packet)
        {
            ModConfigData data = Serializer.ConvertBytesToObject<ModConfigData>(packet.contents);

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
                ModConfigData data = new ModConfigData();
                data._stepMode = ModConfigStepMode.Send;
                data._configFile = new ModConfigFile();
                SortModsIntoCategories(data._configFile, DialogManager.dialogTupleListingResultString, DialogManager.dialogTupleListingResultInt);

                Packet packet = Packet.CreatePacketFromObject(nameof(ModManager), data);
                Network.listener.EnqueuePacket(packet);

                if (isFirstEdit)
                {
                    Page toUse = new Page_SelectScenario();
                    toUse.next = new Page_SelectStartingSite();
                    DialogManager.PushNewDialog(toUse);
                }
                else DialogManager.PushNewDialog(new RT_Dialog_OK("Mod configuration has been changed!"));
            };

            string[] keys = ModManagerHelper.GetRunningModList().UnsortedMods;
            string[] values = new string[] { "Required", "Optional", "Forbidden" };
            RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager" , "Manage mods for the server", keys, values, toDo);
            DialogManager.PushNewDialog(dialog);
        }

        public static void SortModsIntoCategories(ModConfigFile modConfig, string[] modNames, int[] categoryIndexes)
        {
            List<string> requiredMods = new List<string>();
            List<string> optionalMods = new List<string>();
            List<string> forbiddenMods = new List<string>();

            for (int i = 0; i < modNames.Length; i++)
            {
                switch((ModType)categoryIndexes[i])
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
            }

            modConfig.UnsortedMods = modNames;
            modConfig.RequiredMods = requiredMods.ToArray();
            modConfig.OptionalMods = optionalMods.ToArray();
            modConfig.ForbiddenMods = forbiddenMods.ToArray();
        }
    }

    public static class ModManagerHelper
    {
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

        public static void GetConflictingMods(Packet packet)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginData._extraDetails.ToArray()));
        }
    }
}
