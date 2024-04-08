using HarmonyLib;
using RimWorld;

namespace GameClient
{
    [HarmonyPatch(typeof(CompSpawnerFilth), "TrySpawnFilth")]
    public static class PatchFilthDuringVisit
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (!Network.isConnectedToServer) return true;
            else
            {
                if (ClientValues.isInVisit) return false;
                else return true;
            }
        }
    }
}
