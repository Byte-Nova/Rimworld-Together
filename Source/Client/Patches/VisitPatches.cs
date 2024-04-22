using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

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
