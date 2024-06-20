using HarmonyLib;
using RimWorld.Planet;
using System.Linq;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class PatchSettlementJoin
    {
        [HarmonyPrefix]
        public static bool DoPre(Settlement factionBase)
        {
            if (Network.state == NetworkState.Disconnected) return true;

            if (FactionValues.playerFactions.Contains(factionBase.Faction)) return false;
            else return true;
        }
    }
}
