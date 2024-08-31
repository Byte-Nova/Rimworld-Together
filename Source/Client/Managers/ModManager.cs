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
            LoginData loginData = Serializer.ConvertBytesToObject<LoginData>(packet.contents);

            DialogManager.PushNewDialog(new RT_Dialog_Listing("RTModMismatchMenu".Translate(), "RTModMismatchMenuDesc".Translate(),
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
