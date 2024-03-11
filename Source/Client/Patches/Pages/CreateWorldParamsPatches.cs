using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class CreateWorldParamsPatches
    {
        [HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
        public static class PatchWhenHost
        {
            [HarmonyPrefix]
            public static bool DoPre(Rect rect, Page_CreateWorldParams __instance, string ___seedString, float ___planetCoverage, OverallRainfall ___rainfall, OverallTemperature ___temperature, OverallPopulation ___population, List<FactionDef> ___factions, float ___pollution)
            {
                if (!ClientValues.needsToGenerateWorld) return true;
                else
                {
                    Vector2 buttonSize = new Vector2(150f, 38f);
                    Vector2 buttonLocation = new Vector2(rect.xMax - buttonSize.x, rect.yMax - buttonSize.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        __instance.Close();

                        WorldGeneratorManager.SetValuesFromGame(___seedString, ___planetCoverage, ___rainfall,
                            ___temperature, ___population, ___factions, ___pollution);

                        WorldGeneratorManager.GeneratePatchedWorld(true);
                    }

                    return true;
                }
            }

            [HarmonyPostfix]
            public static void DoPost(Rect rect)
            {
                if (!ClientValues.needsToGenerateWorld) return;
                else
                {
                    Text.Font = GameFont.Small;
                    Vector2 buttonSize = new Vector2(150f, 38f);
                    Vector2 buttonLocation = new Vector2(rect.xMax - buttonSize.x, rect.yMax - buttonSize.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Generate")) { }
                }
            }
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "PostOpen")]
        public static class PatchWhenPlayer
        {
            [HarmonyPrefix]
            public static bool DoPre(Page_CreateWorldParams __instance)
            {
                if (ClientValues.needsToGenerateWorld) return true;
                else
                {
                    __instance.Close();

                    WorldGeneratorManager.GeneratePatchedWorld(false);

                    return false;
                }
            }
        }
    }
}
