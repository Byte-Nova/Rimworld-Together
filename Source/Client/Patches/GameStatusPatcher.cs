using HarmonyLib;
using RimWorld.Planet;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;
using Verse;

namespace RimworldTogether.GameClient.Patches
{
    public class GameStatusPatcher
    {
        [HarmonyPatch(typeof(Game), "InitNewGame")]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (Network.Network.isConnectedToServer)
                {
                    PersistentPatches.ForcePermadeath();
                    PersistentPatches.ManageDevOptions();
                    PersistentPatches.ManageGameDifficulty();

                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = __instance.CurrentMap.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.Network.serverListener.SendData(packet);

                    SavePatch.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.Network.isConnectedToServer)
                {
                    PersistentPatches.ForcePermadeath();
                    PersistentPatches.ManageDevOptions();
                    PersistentPatches.ManageGameDifficulty();

                    PlanetBuilder.BuildPlanet();

                    ClientValues.ToggleReadyToPlay(true);
                }
            }
        }

        [HarmonyPatch(typeof(SettleInEmptyTileUtility), "Settle")]
        public static class SettlePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Caravan caravan)
            {
                if (Network.Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = caravan.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.Network.serverListener.SendData(packet);

                    SavePatch.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettleInExistingMapUtility), "Settle")]
        public static class SettleInMapPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Map map)
            {
                if (Network.Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = map.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.Network.serverListener.SendData(packet);

                    SavePatch.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
        public static class AbandonPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Settlement settlement)
            {
                if (Network.Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlement.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.Network.serverListener.SendData(packet);

                    SavePatch.ForceSave();
                }
            }
        }
    }
}
