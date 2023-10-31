using System.Collections.Generic;
using System.Linq;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Verse;

namespace RimworldTogether.GameClient.Managers
{
    public static class ModManager
    {
        public static string[] GetRunningModList()
        {
            List<string> compactedMods = new List<string>();

            ModContentPack[] runningMods = LoadedModManager.RunningMods.ToArray();
            foreach (ModContentPack mod in runningMods) compactedMods.Add(mod.PackageId);
            return compactedMods.ToArray();
        }

        public static void GetConflictingMods(Packet packet)
        {
            LoginDetailsJSON loginDetailsJSON = (LoginDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginDetailsJSON.conflictingMods.ToArray()));
        }

        public static bool CheckIfMapHasConflictingMods(MapDetailsJSON mapDetailsJSON)
        {
            string[] currentMods = GetRunningModList();

            foreach (string mod in mapDetailsJSON.mapMods)
            {
                if (!currentMods.Contains(mod)) return true;
            }

            foreach (string mod in currentMods)
            {
                if (!mapDetailsJSON.mapMods.Contains(mod)) return true;
            }

            return false;
        }
    }
}
