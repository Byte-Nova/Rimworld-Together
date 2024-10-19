using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class PollutionPatch
    {

        [HarmonyPatch(typeof(WorldPollutionUtility), nameof(WorldPollutionUtility.PolluteWorldAtTile))]
        public static class PatchAddPollution
        {
            public static int lastPollutedTile;
            public static bool addedByServer = false;

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(PatchAddPollution), nameof(StoreNumValue));

                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Stloc_0)  
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_0)); 
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, method));
                    }
                }

                return codes.AsEnumerable();
            }

            public static void StoreNumValue(int num)
            {
                lastPollutedTile = num;
            }

            [HarmonyPostfix]
            public static void DoPost(float pollutionAmount)
            {
                if (addedByServer) return;
                if (Network.state == ClientNetworkState.Disconnected) return;

                int id = lastPollutedTile;

                PollutionDetails pollution = new PollutionDetails();
                pollution.tile = id;
                pollution.quantity = pollutionAmount;
                PollutionData data = new PollutionData();
                data._pollutionData = pollution;
                Packet packet = Packet.CreatePacketFromObject(nameof(PollutionManager), data);
                Network.listener.EnqueuePacket(packet);
            }
        }

    }
}
