using GameClient;
using HarmonyLib;
using SaveOurShip2;
using Shared;
using static Shared.CommonEnumerators;

namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(WorldObjectOrbitingShip), nameof(WorldObjectOrbitingShip.ShouldRemoveMapNow))]
    public static class ShipLostAndCrashing
    {
        [HarmonyPostfix]
        public static void DoPost(WorldObjectOrbitingShip __instance)
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                if (__instance.Map.GetComponent<ShipMapComp>().ShipMapState == ShipMapState.burnUpSet)
                {
                    if (GameClient.ClientValues.verboseBool)
                    {
                        Logger.Warning("[SOS2]Player lost ship.");
                    }
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData._settlementData = new SpaceSettlementFile();
                    settlementData._settlementData.Tile = Main.shipTile;
                    Main.shipTile = -1;
                    settlementData._stepMode = SettlementStepMode.Remove;
                    settlementData._settlementData.isShip = true;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }
    }
}
