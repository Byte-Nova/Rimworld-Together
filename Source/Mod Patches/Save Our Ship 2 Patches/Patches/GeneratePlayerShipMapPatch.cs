using HarmonyLib;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient;
using SaveOurShip2;
namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(ShipInteriorMod2), nameof(ShipInteriorMod2.GeneratePlayerShipMap))]
    public static class GeneratePlayerShipMapPost
    {
        [HarmonyPostfix]
        public static void DoPost(Map __result)
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                if (__result != null)
                {
                    ClientValues.ManageDevOptions();
                    DifficultyManager.EnforceCustomDifficulty();
                    Main.shipTile = __result.Tile;
                    PlayerShipManagerHelper.SendSettlementToServer(__result);


                    SaveManager.ForceSave();
                }
            }
        }
    }
}
