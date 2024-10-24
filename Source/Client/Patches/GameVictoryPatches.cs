using System;
using System.Data;
using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using Shared;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(ArchonexusCountdown), "EndGame")]
    public static class Archonexus_Ending_Patch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                var gameVictoryData = new GameVictoryData();
                gameVictoryData._playerName = ClientValues.username;
                gameVictoryData._ending = "Archonexus";
                var packet = Packet.CreatePacketFromObject(nameof(GameVictoryManager), gameVictoryData);
                Network.listener.EnqueuePacket(packet);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ShipCountdown), "CountdownEnded")]
    public static class Ship_Ending_Patch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Connected)
            {
                var gameVictoryData = new GameVictoryData();
                gameVictoryData._playerName = ClientValues.username;
                gameVictoryData._ending = "Left to space by the ship";
                var packet = Packet.CreatePacketFromObject(nameof(GameVictoryManager), gameVictoryData);
                Network.listener.EnqueuePacket(packet);
            }

            return true;
        }

        [HarmonyPatch(typeof(VoidAwakeningUtility), nameof(VoidAwakeningUtility.EmbraceTheVoid))]
        public static class VoidAwakening_Patch
        {
            [HarmonyPrefix]
            public static bool DoPre()
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    var gameVictoryData = new GameVictoryData();
                    gameVictoryData._playerName = ClientValues.username;
                    gameVictoryData._ending = "Awakened the void";
                    var packet = Packet.CreatePacketFromObject(nameof(GameVictoryManager), gameVictoryData);
                    Network.listener.EnqueuePacket(packet);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(VoidAwakeningUtility), nameof(VoidAwakeningUtility.DisruptTheLink))]
        public static class DisruptTheLink_Patch
        {
            [HarmonyPrefix]
            public static bool DoPre()
            {
                if (Network.state == ClientNetworkState.Connected)
                {
                    var gameVictoryData = new GameVictoryData();
                    gameVictoryData._playerName = ClientValues.username;
                    gameVictoryData._ending = "Disrupted the link";
                    var packet = Packet.CreatePacketFromObject(nameof(GameVictoryManager), gameVictoryData);
                    Network.listener.EnqueuePacket(packet);
                }

                return true;
            }
        }
    }
}