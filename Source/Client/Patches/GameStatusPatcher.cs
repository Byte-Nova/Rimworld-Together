using HarmonyLib;
using RimWorld.Planet;
using Shared;
using Verse;

namespace GameClient
{
    public class GameStatusPatcher
    {
        [HarmonyPatch(typeof(Game), "InitNewGame")]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (Network.isConnectedToServer)
                {
                    ClientValues.ForcePermadeath();
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = __instance.CurrentMap.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(Game), "LoadGame")]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.isConnectedToServer)
                {
                    ClientValues.ForcePermadeath();
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    PlanetManager.BuildPlanet();

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
                if (Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = caravan.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettleInExistingMapUtility), "Settle")]
        public static class SettleInMapPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Map map)
            {
                if (Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = map.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Add).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
        public static class AbandonPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Settlement settlement)
            {
                if (Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlement.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)CommonEnumerators.SettlementStepMode.Remove).ToString();

                    Packet packet = Packet.CreatePacketFromJSON("SettlementPacket", settlementDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);

                    SaveManager.ForceSave();
                }
            }
        }
    }
}
