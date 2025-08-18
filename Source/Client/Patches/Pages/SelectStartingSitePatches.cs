using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GameClient.Managers;
using GameClient.Values;
using HarmonyLib;
using RimWorld;
using UnityEngine.SceneManagement;
using static Shared.CommonEnumerators;

namespace GameClient.Patches.Pages
{
    public class SelectStartingSitePatches
    {
        [HarmonyPatch(typeof(Page_SelectStartingSite), "DoCustomBottomButtons")]
        public static class PathSelectStartingSitePage
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
            {
                const string disconnectText = "Disconnect";
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                MethodInfo helper = AccessTools.Method(typeof(PathSelectStartingSitePage), nameof(Helper));
                int index = 0;
                for (; index < codes.Count; index++)
                {
                    if (codes[index].operand is string str && str == "Back")
                    {
                        CodeInstruction[] newInstructions = new CodeInstruction[]{
                            new(OpCodes.Ldstr, disconnectText) // Swap the text
                        };
                        
                        TranspilerHelper.CheckIfConnected(ilGenerator, codes, newInstructions, ref index, 3);
                        break;
                    }
                }

                bool flag = false;
                for (; index < codes.Count; index++)
                {
                    if (codes[index].opcode == OpCodes.Ldarg_0)
                    {
                        if (!flag)
                        {
                            flag = true;
                            continue;
                        }

                        CodeInstruction[] newInstructions = new CodeInstruction[]
                        {
                            new(OpCodes.Call, helper), // Call helper so Nova can read it
                            new(OpCodes.Ret) // Return
                        };
                        TranspilerHelper.CheckIfConnected(ilGenerator, codes, newInstructions, ref index, 0);
                        break;
                    }
                }

                return codes;
            }

            private static void Helper()
            {
                SceneManager.LoadScene(0);
                DisconnectionManager.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.QuitToMenu);
                ClientNetwork.Instance.ClientListener.DisconnectFlag = true;
            }
        }

        [HarmonyPatch(typeof(Page_SelectStartingSite), "PreOpen")]
        public static class PatchSettlements
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return;

                if (!ClientValues.IsGeneratingFreshWorld)
                {
                    WorldManager.SetPlanetFeatures();
                    WorldManager.SetPlanetFactions();
                }

                PlanetManager.BuildPlanet();
            }
        }
    }
}
