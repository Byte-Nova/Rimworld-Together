using GameClient;
using HarmonyLib;
using RimWorld.Planet;
using SaveOurShip2;
using Shared;
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
                        settlementData._settlementData = new SettlementFile();
                        settlementData._settlementData.Tile = target.Tile;
                        settlementData._stepMode = SettlementStepMode.Add;
                        settlementData._settlementData.isShip = false;

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
                        settlementData._settlementData = new SpaceSettlementFile();
                        settlementData._settlementData.Tile = tile;
                        settlementData._stepMode = SettlementStepMode.Remove;
                        settlementData._settlementData.isShip = false;

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
