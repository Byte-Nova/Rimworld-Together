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
                if (Network.state == NetworkState.Connected)
                {
                    ClientValues.ManageDevOptions();
                    CustomDifficultyManager.EnforceCustomDifficulty();
                    if (SOS2SendData.IsMapShip(__instance.CurrentMap).Result == false)
                    {
                        Logger.Warning("True");
                        SettlementData settlementData = new SettlementData();
                        settlementData.tile = __instance.CurrentMap.Tile;
                        settlementData.settlementStepMode = SettlementStepMode.Add;

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SettlementPacket), settlementData);
                        Network.listener.EnqueuePacket(packet);

                        SaveManager.ForceSave();
                    }
                }
                if (ClientValues.isGeneratingFreshWorld)
                {
                    WorldGeneratorManager.SendWorldToServer();
                    ClientValues.ToggleGenerateWorld(false);
                }
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        public static class LoadModePatch
        {
            [HarmonyPostfix]
            public static void GetIDFromExistingGame()
            {
                if (Network.state == NetworkState.Connected)
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
                if (Network.state == NetworkState.Connected)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = caravan.Tile;
                    settlementData.settlementStepMode = SettlementStepMode.Add;

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
                if (Network.state == NetworkState.Connected)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = map.Tile;
                    settlementData.settlementStepMode = SettlementStepMode.Add;

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
                if (Network.state == NetworkState.Connected)
                {
                    SettlementData settlementData = new SettlementData();
                    settlementData.tile = settlement.Tile;
                    settlementData.settlementStepMode = SettlementStepMode.Remove;

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
                if (Network.state == NetworkState.Connected)
                {
                    if (!ClientValues.isReadyToPlay) return;

                    if (__instance.Faction == Faction.OfPlayer) return;
                    else if (FactionValues.playerFactions.Contains(__instance.Faction)) return;
                    else NPCSettlementManager.RequestSettlementRemoval(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
        public static class PatchDevMode
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (Network.state == NetworkState.Connected) ClientValues.ManageDevOptions();
                else return;
            }
        }

        [HarmonyPatch(typeof(Page_SelectStorytellerInGame), nameof(Page_SelectStorytellerInGame.DoWindowContents))]
        public static class PatchCustomDifficulty
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (Network.state == NetworkState.Connected) CustomDifficultyManager.EnforceCustomDifficulty();
                else return;
            }
        }
    }
}
