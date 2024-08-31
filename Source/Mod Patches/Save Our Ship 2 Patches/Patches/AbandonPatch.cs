using GameClient;
using HarmonyLib;
using RT_SOS2Patches.Master;
using SaveOurShip2;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace RT_SOS2Patches
{
    [HarmonyPatch(typeof(WorldObjectOrbitingShip), nameof(WorldObjectOrbitingShip.Abandon))]
    public static class ShipAbandonPatch
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                if (GameClient.ClientValues.verboseBool)
                {
                    Logger.Warning("[SOS2]Player abandoned ship.");
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
