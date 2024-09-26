using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class GameStatusPatches
    {
        [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    ClientValues.ManageDevOptions();
                    DifficultyManager.EnforceCustomDifficulty();

                    SaveManager.ForceSave();

                    if (ClientValues.isGeneratingFreshWorld)
                    {
                        PlanetGeneratorManager.SendWorldToServer();
                        ClientValues.ToggleGenerateWorld(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    ClientValues.ManageDevOptions();
                    DifficultyManager.EnforceCustomDifficulty();

                    PlanetManager.BuildPlanet();
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
                if (Network.state == ClientNetworkState.Connected) ClientValues.ManageDevOptions();
                else return;
            }
        }

        [HarmonyPatch(typeof(Page_SelectStorytellerInGame), nameof(Page_SelectStorytellerInGame.DoWindowContents))]
        public static class PatchCustomDifficulty
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (Network.state == ClientNetworkState.Connected) DifficultyManager.EnforceCustomDifficulty();
                else return;
            }
        }
    }
}
