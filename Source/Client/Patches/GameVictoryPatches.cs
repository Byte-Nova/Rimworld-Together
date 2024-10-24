using HarmonyLib;
using RimWorld;
using Shared;

namespace GameClient
{
    public class EndGamePatches
    {
        [HarmonyPatch(typeof(ArchonexusCountdown), "EndGame")]
        public static class Archonexus_Ending_Patch
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                GameVictoryData gameVictoryData = new GameVictoryData();
                gameVictoryData._playerName = ClientValues.username;
                gameVictoryData._ending = "Archonexus";
                Packet packet = Packet.CreatePacketFromObject(nameof(GameVictoryManager), gameVictoryData);
            }
        }
            
    }
}