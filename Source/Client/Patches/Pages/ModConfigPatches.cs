using HarmonyLib;
using RimWorld;
using static Shared.CommonEnumerators;
using GameClient.Dialogs;
using GameClient.Misc;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Dialog_Options), "DoModOptions")]
    public static class Patch_DialogOptions_DoModOptions
    {
        public static bool executedMessage;

        [HarmonyPrefix]
        public static bool DoPre(Dialog_Options __instance)
        {
            if (!SessionHandler.CurrentModConfig.IsEnforced) return true;
            else if (SessionHandler.IsAdmin) return true;
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