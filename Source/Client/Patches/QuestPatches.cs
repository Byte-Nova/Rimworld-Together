using System;
using System.Linq;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Add))]
    public static class PatchAddPollution
    {
        [HarmonyPrefix]
        public static bool DoPre(Quest quest)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;

            foreach (Faction faction in ClientValues.PlayerFactions)
            {
                if (quest.InvolvedFactions.Contains(faction))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
