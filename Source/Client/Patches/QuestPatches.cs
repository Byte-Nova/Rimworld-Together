using System;
using System.Linq;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using static Shared.CommonEnumerators;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Add))]
    public static class PatchAddQuest
    {
        [HarmonyPrefix]
        public static bool DoPre(Quest quest)
        {
            foreach (Faction faction in SessionHandler.PlayerFactions)
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
