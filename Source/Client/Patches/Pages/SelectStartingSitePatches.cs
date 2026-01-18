using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GameClient.Hooks.TCPNetwork;
using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using UnityEngine.SceneManagement;
using static Shared.CommonEnumerators;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Page_SelectStartingSite), "PreOpen")]
    public static class PatchSettlements
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (!SessionHandler.IsGeneratingFreshWorld)
            {
                WorldManager.SetPlanetFeatures();
                WorldManager.SetPlanetFactions();
            }

            PlanetManager.BuildPlanet();
        }
    }

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
                    codes[index] = new CodeInstruction(OpCodes.Ldstr, disconnectText);
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
                    codes.InsertRange(index,  [
                    new(OpCodes.Call, helper),
                    new (OpCodes.Ret)]);
                    break;
                }
            }

            return codes;
        }

        private static void Helper()
        {
            SceneManager.LoadScene(0);
            ClientNetwork.Instance.ClientListener.DisconnectNow();
        }
    }
}
