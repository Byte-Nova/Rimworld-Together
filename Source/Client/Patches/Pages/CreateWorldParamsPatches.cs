using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

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
                if (Network.state == ClientNetworkState.Disconnected) return true;
                if (!ClientValues.isGeneratingFreshWorld) return true;

                Vector2 buttonSize = new Vector2(150f, 38f);
                Vector2 buttonLocation = new Vector2(rect.xMax - buttonSize.x, rect.yMax - buttonSize.y);
                if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                {
                    __instance.Close();

                    ___factions.Add(FactionValues.neutralPlayerDef);
                    ___factions.Add(FactionValues.allyPlayerDef);
                    ___factions.Add(FactionValues.enemyPlayerDef);
                    ___factions.Add(FactionValues.yourOnlineFactionDef);

                    PlanetGeneratorManager.SetValuesFromGame(___seedString, ___planetCoverage, ___rainfall, 
                        ___temperature, ___population, ___factions, ___pollution);

                    PlanetGeneratorManager.GeneratePatchedWorld();
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Page_CreateWorldParams), "PostOpen")]
        public static class PatchWhenPlayer
        {
            [HarmonyPrefix]
            public static bool DoPre(Page_CreateWorldParams __instance)
            {
                if (Network.state == ClientNetworkState.Disconnected) return true;
                if (ClientValues.isGeneratingFreshWorld) return true;

                __instance.Close();

                PlanetGeneratorManager.GeneratePatchedWorld();

                return false;
            }
        }
    }
}
