using HarmonyLib;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using GameClient;
using SaveOurShip2;
using RimWorld.Planet;
using System.Linq;
namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(ShipInteriorMod2), nameof(ShipInteriorMod2.MoveShip))]
    public static class LandShipCheckPost
    {
        [HarmonyPostfix]
        public static void DoPost(Building core, Map targetMap)
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                ClientValues.ManageDevOptions();
                CustomDifficultyManager.EnforceCustomDifficulty();
                Map map = core.Map;
                if (!targetMap.IsSpace() && ShipInteriorMod2.FindPlayerShipMap() == null)
                {
                    if (GameClient.ClientValues.verboseBool)
                    {
                        Logger.Warning("[SOS2]Deleting empty space map");
                    }
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData.settlementData = new OnlineSpaceSettlementFile();
                    settlementData.settlementData.tile = Main.shipTile;
                    Main.shipTile = -1;
                    settlementData.stepMode = SettlementStepMode.Remove;
                    settlementData.settlementData.isShip = true;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                } 
            }
        }
    }
}
