using GameClient.Values;
using HarmonyLib;
using RimWorld;
using static Shared.CommonEnumerators;
using GameClient.Dialogs;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Dialog_Options), "DoModOptions")]
    public static class Patch_DialogOptions_DoModOptions
    {
        public static bool executedMessage;

        [HarmonyPrefix]
        public static bool DoPre(Dialog_Options __instance)
        {
            if (SessionValues.CurrentNetworkState == ClientNetworkState.Disconnected) return true;
            else if (!SessionValues.ConfigFile.EnforcedConfigs) return true;
            else if (ClientValues.IsAdmin) return true;
            else
            {
                __instance.Close();

                if (!executedMessage)
                {
                    executedMessage = true;

                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("Error",
                        new string[] { "Mod options can't be changed in this server!" },
                        delegate { executedMessage = false; }));
                }

                return false;
            }
        }
    }
}