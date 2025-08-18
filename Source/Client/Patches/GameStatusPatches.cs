using GameClient.Managers;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;
using GameClient.Misc;
using GameClient.Dialogs;
using System;

namespace GameClient.Patches
{
    public class GameStatusPatches
    {
        [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected)
                {
                    SettlementManager.SendNewPlayerSettlement(__instance.CurrentMap.Tile);

                    if (!ClientValues.IsGeneratingFreshWorld) SaveManager.ForceSave();
                    else ModManager.OpenModManagerMenu(true);

                    ClientValues.ForcePermadeath();
                    ClientValues.ToggleReadyToPlay(true);
                }
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected)
                {
                    PlanetManager.BuildPlanet();

                    GameParameterManager.SetScenario(SessionValues.ScenarioFile);
                    GameParameterManager.SetStoryteller(SessionValues.StorytellerFile);
                    GameParameterManager.SetDifficulty(SessionValues.DifficultyFile);

                    ClientValues.ForcePermadeath();
                    ClientValues.ToggleReadyToPlay(true);
                }
            }
        }

        [HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
        public static class PatchDevMode
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (SessionValues.CurrentNetworkState == ClientNetworkState.Connected) ClientValues.ManageDevOptions();
                else return;
            }
        }
    }
}
