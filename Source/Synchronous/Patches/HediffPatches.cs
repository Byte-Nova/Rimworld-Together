using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using Synchronous.Core;
using Synchronous.Managers;
using Synchronous.Misc;
using Synchronous.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Synchronous.Patches
{
    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult)])]
    public static class P_Pawn_HealthTracker_AddHediff
    {
        [HarmonyPrefix]
        public static bool AddHediff(Hediff hediff, BodyPartRecord part, DamageInfo dinfo, Pawn ___pawn)
        {
            if (PatchHandler.BypassFlag) return true;
            else if (!Main_.CheckIfShouldPatch(___pawn.MapHeld)) return true;
            else if (!SessionHandler.IsSynchronousHost) return false;
            else
            {
                BodyPartRecord bodyPart = part != null ? part : dinfo.HitPart;
                SHediffManager.Ask(hediff, bodyPart, ___pawn, PlayerHediff.HediffMode.Add);
                return false;
            }
        }
    }

    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
    public static class P_Pawn_HealthTracker_RemoveHediff
    {
        [HarmonyPrefix]
        public static bool RemoveHediff(Hediff hediff, Pawn ___pawn)
        {
            if (PatchHandler.BypassFlag) return true;
            else if (!Main_.CheckIfShouldPatch(___pawn.MapHeld)) return true;
            else if (!SessionHandler.IsSynchronousHost) return false;
            else
            {
                SHediffManager.Ask(hediff, hediff.Part, ___pawn, PlayerHediff.HediffMode.Remove);
                return false;
            }
        }
    }

    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(HediffComp_TendDuration), nameof(HediffComp_TendDuration.CompTended))]
    public static class P_HediffComp_TendDuration_CompTended
    {
        [HarmonyPrefix]
        public static bool CompTended(float quality, float maxQuality, HediffComp_TendDuration __instance, ref float ___totalTendQuality, int batchPosition)
        {
            if (PatchHandler.BypassFlag)
            {
                __instance.tendQuality = quality;
                ___totalTendQuality += __instance.tendQuality;

                if (__instance.TProps.TendIsPermanent) __instance.tendTicksLeft = 1;
                else __instance.tendTicksLeft = Mathf.Max(0, __instance.tendTicksLeft) + __instance.TProps.TendTicksFull;

                if (batchPosition == 0 && __instance.Pawn.Spawned)
                {
                    string text = "TextMote_Tended".Translate(__instance.parent.Label).CapitalizeFirst() + "\n" + "Quality".Translate() +
                        " " + __instance.tendQuality.ToStringPercent();

                    MoteMaker.ThrowText(__instance.Pawn.DrawPos, __instance.Pawn.Map, text, Color.white, 3.65f);
                }

                __instance.Pawn.health.Notify_HediffChanged(__instance.parent);

                return false;
            }

            else
            {
                if (!SessionHandler.IsSynchronousHost) return false;
                else
                {
                    float tendQuality = Mathf.Clamp(quality + Rand.Range(-0.25f, 0.25f), 0f, maxQuality);
                    SHediffManager.Ask((Hediff)__instance.parent, __instance.parent.Part, __instance.Pawn, PlayerHediff.HediffMode.Tend, tendQuality);
                    return false;
                }
            }
        }
    }
}
