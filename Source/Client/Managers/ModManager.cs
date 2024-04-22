using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GameClient
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
            JoinDetailsJSON loginDetailsJSON = (JoinDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("Mod Conflicts", "The following mods are conflicting with the server",
                loginDetailsJSON.extraDetails.ToArray()));
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
