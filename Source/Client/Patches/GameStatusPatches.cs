using GameClient.Managers;
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
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static class InitModePatch
    {
        [HarmonyPostfix]
        public static void ModifyPost(Game __instance)
        {
            SettlementManager.SendNewPlayerSettlement(__instance.CurrentMap.Tile);

            if (!SessionHandler.IsGeneratingFreshWorld) SaveManager.ForceSave();
            else ModManager.OpenModManagerMenu(true);

            SessionHandler.IsReadyToPlay = true;

            MainThreadHandler.Instance.DoOnStartMethods();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class LoadModePatch
    {
        [HarmonyPostfix]
        public static void GetIDFromExistingGame()
        {
            PlanetManager.BuildPlanet();

            GameParameterManager.SetScenario(SessionHandler.CurrentScenario);
            GameParameterManager.SetStoryteller(SessionHandler.CurrentStoryteller);
            GameParameterManager.SetDifficulty(SessionHandler.CurrentDifficulty);

            SessionHandler.IsReadyToPlay = true;

            MainThreadHandler.Instance.DoOnStartMethods();
        }
    }
}
