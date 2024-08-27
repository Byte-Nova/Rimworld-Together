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
            if (Network.state == NetworkState.Connected)
            {
                if (__result != null)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    ShipMapComp comp = __result.GetComponent<ShipMapComp>();
                    WorldObjectOrbitingShip orbitShip = comp.mapParent;
                    SpaceSettlementData spaceSiteData = new SpaceSettlementData();

                    spaceSiteData.isShip = true;
                    spaceSiteData.name = comp.mapParent.Name;
                    spaceSiteData.tile = __result.Tile;
                    spaceSiteData.settlementStepMode = SettlementStepMode.Add;
                    spaceSiteData.phi = orbitShip.Phi;
                    spaceSiteData.theta = orbitShip.Theta;
                    spaceSiteData.radius = orbitShip.Radius;

                    Packet packet = Packet.CreatePacketFromObject(nameof(GameClient.PacketHandler.SpaceSettlementPacket), spaceSiteData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }
    }
}
