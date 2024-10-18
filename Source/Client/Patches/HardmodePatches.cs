using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace GameClient
{
    public class HardmodePatches
    {
        public static List<string> forceSavedThreats = new List<string>() {"ThreatBig"};

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Pawn_Kill_Patches
        {
            [HarmonyPostfix]
            public static void DoPost(Pawn __instance)
            {
                if (!SessionValues.actionValues.HardcoreMode) return;
                if (__instance.Faction != null && __instance.Faction.IsPlayer)
                    SaveManager.ForceSave();
            }
        }

        [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), new[] {typeof(Letter), typeof(string), typeof(int), typeof(bool)})]
        public static class Threat_Patches
        {
            [HarmonyPostfix]
            public static void DoPost(Letter let)
            {
                if (!SessionValues.actionValues.HardcoreMode) return;
                if (forceSavedThreats.Contains(let.def.defName))
                    SaveManager.ForceSave();
            }
        }
    }
}