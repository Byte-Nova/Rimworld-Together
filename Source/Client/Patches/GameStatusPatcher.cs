using HarmonyLib;
using RimWorld.Planet;
using Shared.JSON;
using Shared.Misc;
using Verse;

namespace RimworldTogether
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
                    PersistentPatches.ForcePermadeath();
                    PersistentPatches.ManageDevOptions();
                    PersistentPatches.ManageGameDifficulty();

                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = __instance.CurrentMap.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)SettlementManager.SettlementStepMode.Add).ToString();

                    string[] contents = new string[] { Serializer.SerializeToString(settlementDetailsJSON) };
                    Packet packet = new Packet("SettlementPacket", contents);
                    Network.SendData(packet);

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
                if (Network.isConnectedToServer)
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
                if (Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = caravan.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)SettlementManager.SettlementStepMode.Add).ToString();

                    string[] contents = new string[] { Serializer.SerializeToString(settlementDetailsJSON) };
                    Packet packet = new Packet("SettlementPacket", contents);
                    Network.SendData(packet);

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
                if (Network.isConnectedToServer)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlement.Tile.ToString();
                    settlementDetailsJSON.settlementStepMode = ((int)SettlementManager.SettlementStepMode.Remove).ToString();

                    string[] contents = new string[] { Serializer.SerializeToString(settlementDetailsJSON) };
                    Packet packet = new Packet("SettlementPacket", contents);
                    Network.SendData(packet);

                    SavePatch.ForceSave();
                }
            }
        }
    }
}
