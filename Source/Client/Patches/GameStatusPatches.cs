﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public class GameStatusPatches
    {
        [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
        public static class InitModePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Game __instance)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData.settlementData.Tile = __instance.CurrentMap.Tile;
                    settlementData.stepMode = SettlementStepMode.Add;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();

                    if (ClientValues.isGeneratingFreshWorld)
                    {
                        PlanetGeneratorManager.SendWorldToServer();
                        ClientValues.ToggleGenerateWorld(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();

                    PlanetManager.BuildPlanet();
                    ClientValues.ToggleReadyToPlay(true);
                }
            }
        }

        [HarmonyPatch(typeof(SettleInEmptyTileUtility), nameof(SettleInEmptyTileUtility.Settle))]
        public static class SettlePatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Caravan caravan)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData.settlementData.Tile = caravan.Tile;
                    settlementData.stepMode = SettlementStepMode.Add;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(SettleInExistingMapUtility), nameof(SettleInExistingMapUtility.Settle))]
        public static class SettleInMapPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Map map)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData.settlementData.Tile = map.Tile;
                    settlementData.stepMode = SettlementStepMode.Add;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

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
                if (Network.state == ClientNetworkState.Connected)
                {
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData.settlementData.Tile = settlement.Tile;
                    settlementData.stepMode = SettlementStepMode.Remove;

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                    Network.listener.EnqueuePacket(packet);

                    SaveManager.ForceSave();
                }
            }
        }

        [HarmonyPatch(typeof(Settlement), nameof(Settlement.PostRemove))]
        public static class DestroyNPCSettlementPatch
        {
            [HarmonyPostfix]
            public static void ModifyPost(Settlement __instance)
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    if (!ClientValues.isReadyToPlay) return;

                    if (__instance.Faction == Faction.OfPlayer) return;
                    else if (FactionValues.playerFactions.Contains(__instance.Faction)) return;
                    else if (NPCSettlementManagerHelper.lastRemovedSettlement != __instance) NPCSettlementManager.RequestSettlementRemoval(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
        public static class PatchDevMode
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (Network.state == ClientNetworkState.Connected) ClientValues.ManageDevOptions();
                else return;
            }
        }

        [HarmonyPatch(typeof(Page_SelectStorytellerInGame), nameof(Page_SelectStorytellerInGame.DoWindowContents))]
        public static class PatchCustomDifficulty
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (Network.state == ClientNetworkState.Connected) CustomDifficultyManager.EnforceCustomDifficulty();
                else return;
            }
        }
    }
}
