using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace GameClient
{
    public class HardmodePatches
    {
        public static List<string> forceSavedThreats = new List<string>() {"ThreatBig", "ThreatSmall", "GameEnded"};

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class PawnDeathPatches
        {
            [HarmonyPostfix]
            public static void DoPost(Pawn __instance)
            {
                if (!SessionValues.actionValues.HardcoreMode) return;
                if ((__instance.Faction != null && __instance.Faction.IsPlayer && __instance.RaceProps.Humanlike)|| __instance.IsPrisoner)
                    SaveManager.ForceSave();
            }
        }

        [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), new[] {typeof(Letter), typeof(string), typeof(int), typeof(bool)})]
        public static class ThreatPatches
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

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class DownedPatches
    {
        [HarmonyPostfix]
        public static void DoPost(Pawn_HealthTracker __instance)
        {
            if (!SessionValues.actionValues.HardcoreMode) return;
            foreach (Map map in Find.Maps.Where(map => map.IsPlayerHome))
            {
                foreach (Pawn colonist in map.mapPawns.FreeColonists)
                {
                    if (!colonist.Downed)
                        return;
                }

                SaveManager.ForceSave();
            }
        }
    }

    [HarmonyPatch(typeof(BossgroupWorker), nameof(BossgroupWorker.Resolve))]
    public static class BossPatches
    {
        [HarmonyPostfix]
        public static void DoPost(BossgroupWorker __instance)
        {
            if (!SessionValues.actionValues.HardcoreMode) return;
            if (!ModsConfig.BiotechActive) return;
                SaveManager.ForceSave();
        }
    }
    [HarmonyPatch(typeof(GameComponent_Anomaly), nameof(GameComponent_Anomaly.SetLevel))]
    public static class MonolithSetLevelPatches
    {
        [HarmonyPostfix]
        public static void DoPost(BossgroupWorker __instance)
        {
            if (!SessionValues.actionValues.HardcoreMode) return;
            if (!ModsConfig.AnomalyActive) return;
                SaveManager.ForceSave();
        }
    }
    [HarmonyPatch(typeof(GameComponent_Anomaly), nameof(GameComponent_Anomaly.IncrementLevel))]
    public static class MonolithIncrementLevelPatches
    {
        [HarmonyPostfix]
        public static void DoPost(BossgroupWorker __instance)
        {
            if (!SessionValues.actionValues.HardcoreMode) return;
            if (!ModsConfig.AnomalyActive) return;
                SaveManager.ForceSave();
        }
    }

    [HarmonyPatch(typeof(PsychicRitualToil), nameof(PsychicRitual.Start))]
    public static class RitualStartPatches
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (!SessionValues.actionValues.HardcoreMode) return;
            if (!ModsConfig.AnomalyActive) return;
                SaveManager.ForceSave();
        }
    }
}