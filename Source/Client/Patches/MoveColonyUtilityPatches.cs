using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GameClient.Managers;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using Verse;
using TCPNetwork.Packets;

namespace GameClient.Patches
{
    public static class MoveColonyUtilityPatches
    {
        [HarmonyPatch(typeof(MoveColonyUtility), nameof(MoveColonyUtility.MoveColonyAndReset))]
        public static class MoveColonyAndResetPatch
        {
            /// <summary>
            /// Catches the removed player settlements and notifies the server about them
            /// </summary>
            /// 

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                MethodInfo method = AccessTools.Method(typeof(MoveColonyAndResetPatch), nameof(RemovePreviousSettlements));
                MethodInfo methodToCheck = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.IdeologyActive));
            
                FieldInfo PlayerSettlementsRemoved =
                    AccessTools.Field(typeof(MoveColonyUtility), "playerSettlementsRemoved");
                
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == methodToCheck)
                    {
                        codes.InsertRange(i, new []
                        {
                            new CodeInstruction(OpCodes.Ldsfld,  PlayerSettlementsRemoved),
                            new CodeInstruction(OpCodes.Call, method)
                        });
                        break;
                    }
                }
                
                return codes.AsEnumerable();
            }

            [HarmonyPostfix]
            public static void Postfix(PlanetTile tile)
            {
                SettlementManager.SendNewPlayerSettlement(tile);
            }

            private static void RemovePreviousSettlements(List<PlanetTile> settlementsToRemove)
            {
                foreach (int settlement in settlementsToRemove)
                {
                    PlayerSettlementData settlementData = new PlayerSettlementData();
                    settlementData._settlementFile.Tile = settlement;
                    settlementData._stepMode = CommonEnumerators.SettlementStepMode.Remove;

                    ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.SettlementManager, settlementData);
                }
            }
        }
    }
}