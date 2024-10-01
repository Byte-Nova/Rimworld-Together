using GameClient;
using HarmonyLib;
using Verse;
using static Shared.CommonEnumerators;
namespace RT_SOS2Patches {
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class LoadModePatch
    {
        [HarmonyPostfix]
        public static void GetIDFromExistingGame()
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                Main.GetShipTile();
            }
        }
    }
}