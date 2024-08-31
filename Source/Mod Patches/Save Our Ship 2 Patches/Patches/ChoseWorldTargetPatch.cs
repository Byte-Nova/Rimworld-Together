using GameClient;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
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
    public static class ChoseWorldTargetPatch
    {
        private static int tile = -1;
        [HarmonyPatch(typeof(Building_ShipSensor), "ChoseWorldTarget")]
        public static class ChoseTargetPatch
        {
            [HarmonyPostfix]
            public static void DoPost(GlobalTargetInfo target, bool __result)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    if (GameClient.ClientValues.verboseBool)
                    {
                        Logger.Warning($"[SOS2]Is observing {target.Tile}");
                    }
                    if (target.WorldObject == null && !Find.World.Impassable(target.Tile))
                    {
                        PlayerSettlementData settlementData = new PlayerSettlementData();
                        settlementData.settlementData = new SettlementFile();
                        settlementData.settlementData.Tile = target.Tile;
                        settlementData.stepMode = SettlementStepMode.Add;
                        settlementData.settlementData.isShip = false;

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                        Network.listener.EnqueuePacket(packet);

                        SaveManager.ForceSave();
                        tile = target.Tile;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Building_ShipSensor), "PossiblyDisposeOfObservedMap")]
        public static class DeleteChosenTargetPatch
        {
            [HarmonyPostfix]
            public static void DoPost(Building_ShipSensor __instance)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    if (GameClient.ClientValues.verboseBool)
                    {
                        Logger.Warning($"[SOS2]Has stopped observing {tile}");
                    }
                    if (tile != -1)
                    {
                        PlayerSettlementData settlementData = new PlayerSettlementData();
                        settlementData.settlementData = new SpaceSettlementFile();
                        settlementData.settlementData.Tile = tile;
                        settlementData.stepMode = SettlementStepMode.Remove;
                        settlementData.settlementData.isShip = false;

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                        Network.listener.EnqueuePacket(packet);

                        SaveManager.ForceSave();
                    }
                }
                tile = -1;
            }
        }
    }
}
