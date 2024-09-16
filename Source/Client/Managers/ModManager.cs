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
                case ModConfigStepMode.Send:
                    //DO
                    break;

                case ModConfigStepMode.Receive:
                    //DO
                    break;

                case ModConfigStepMode.Ask:
                    OpenModManagerMenu(false, data._configFile);
                    break;

                case ModConfigStepMode.Mismatch:
                    //DO
                    break;
            }
        }

        public static void OpenModManagerMenu(bool isFirstEdit, ModConfigFile configFile = null)
        {
            Action toDo = delegate
            {
                ModConfigData data = new ModConfigData();
                data._stepMode = ModConfigStepMode.Send;
                data._configFile = new ModConfigFile();
                data._configFile.Mods = DialogManager.dialogTupleListingResultString;
                data._configFile.Categories = DialogManager.dialogTupleListingResultInt;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ModPacket), data);
                Network.listener.EnqueuePacket(packet);

                if (isFirstEdit)
                {
                    Page toUse = new Page_SelectScenario();
                    toUse.next = new Page_SelectStartingSite();
                    DialogManager.PushNewDialog(toUse);
                }
            };

            string[] keys;
            if (isFirstEdit) keys = GetRunningModList();
            else keys = configFile.Mods;

            string[] values = new string[] { "Required", "Optional", "Forbidden" };

            RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager" , "Manage mods for the server", keys, values, toDo);
            DialogManager.PushNewDialog(dialog);
        }

        public static string[] GetRunningModList()
        {
            List<string> compactedMods = new List<string>();

            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            foreach (ModContentPack mod in runningMods) compactedMods.Add(mod.PackageId);
            return compactedMods.ToArray();
        }

        public static void GetConflictingMods(Packet packet)
        {
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginData._extraDetails.ToArray()));
        }

        public static bool CheckIfMapHasConflictingMods(MapData mapData)
        {
            string[] currentMods = GetRunningModList();

            foreach (string mod in mapData._mapMods)
            {
                if (!currentMods.Contains(mod)) return true;
            }

            foreach (string mod in currentMods)
            {
                if (!mapData._mapMods.Contains(mod)) return true;
            }

            return false;
        }
    }
}
