using HarmonyLib;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using Shared.Details.Planet;
using GameClient.Misc;
using GameClient.Hooks.TCPNetwork;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(WorldPollutionUtility), nameof(WorldPollutionUtility.PolluteWorldAtTile))]
    public static class PatchAddPollution
    {
        private static int lastPollutedTile;

        public static bool addedByServer;

        public static void StoreNumValue(PlanetTile num) { lastPollutedTile = num; }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo method = AccessTools.Method(typeof(PatchAddPollution), nameof(StoreNumValue));

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    i++;
                    codes.InsertRange(i, new CodeInstruction[]
                    {
                            new (OpCodes.Ldloc_0),
                            new (OpCodes.Call, method)
                    });
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        [HarmonyPostfix]
        public static void DoPost(float pollutionAmount)
        {
            if (!SessionHandler.CurrentActionValues.EnablePollutionSpread) return;
            else if (addedByServer) addedByServer = false;
            else
            {
                PollutionDetail pollution = new PollutionDetail();
                pollution.Tile = lastPollutedTile;
                pollution.Quantity = pollutionAmount;

                PollutionData data = new PollutionData();
                data._pollutionData = pollution;

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.PollutionManager, data);
            }
        }
    }
}
