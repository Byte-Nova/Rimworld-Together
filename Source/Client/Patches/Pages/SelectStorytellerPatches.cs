using GameClient.Dialogs;
using GameClient.Hooks.TCPNetwork;
using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using Shared.Misc;
using System;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(Page_SelectStoryteller), nameof(Page_SelectStoryteller.PreOpen))]
    public static class Patch_Page_SelectStoryteller_PreOpen
    {
        [HarmonyPrefix]
        public static bool DoPre(ref DifficultyDef ___difficulty, ref Difficulty ___difficultyValues)
        {
            Find.GameInitData.permadeathChosen = true;
            Find.GameInitData.permadeath = true;

            if (!SessionHandler.IsGeneratingFreshWorld)
            {
                ___difficulty = DifficultyDefOf.Rough;
                ___difficultyValues = new Difficulty(___difficulty);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Page_SelectStoryteller), nameof(Page_SelectStoryteller.DoWindowContents))]
    public static class Patch_Page_SelectStoryteller_DoWindowContents
    {
        public static bool executedMessage;

        [HarmonyPrefix]
        public static bool DoPre(Rect rect, Page_SelectStoryteller __instance)
        {
            if (Widgets.ButtonText(RT_Dialog_Base.GetRectForLocation(rect, RT_Dialog_Base.SmallButtonSize, RT_Dialog_Base.RectLocation.BottomLeft), "") || KeyBindingDefOf.Cancel.KeyDownEvent)
            {
                __instance.Close();
                ClientNetwork.Instance.ClientListener.DisconnectNow();
            }
            
            if (!SessionHandler.IsGeneratingFreshWorld && SessionHandler.CurrentStoryteller.IsEnforced)
            {
                if (executedMessage) return true;
                else
                {
                    Printer.Warning(3);

                    Action toDo = delegate
                    {
                        GameParameterManager.SetStoryteller(SessionHandler.CurrentStoryteller);
                        GameParameterManager.SetDifficulty(SessionHandler.CurrentDifficulty, true);
                        RT_Dialog_Base.PushNewDialog(__instance.next);
                        __instance.Close();

                        executedMessage = false;
                    };
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Storyteller will be forced by the server" }, toDo));

                    executedMessage = true;
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost(Rect rect)
        {
            Text.Font = GameFont.Small;

            if (Widgets.ButtonText(RT_Dialog_Base.GetRectForLocation(rect, RT_Dialog_Base.SmallButtonSize,
                RT_Dialog_Base.RectLocation.BottomLeft), "Disconnect")) { };
        }
    }

    [HarmonyPatch(typeof(Page_SelectStorytellerInGame), nameof(Page_SelectStorytellerInGame.PreClose))]
    public static class Patch_Page_SelectStorytellerInGame_PreClose
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (SessionHandler.IsAdmin)
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Difficulty settings overriden due to being an admin" }));
                return true;
            }

            if (SessionHandler.CurrentDifficulty.IsEnforced || SessionHandler.CurrentStoryteller.IsEnforced)
            {
                Action toDo = delegate
                {
                    GameParameterManager.SetStoryteller(SessionHandler.CurrentStoryteller);
                    GameParameterManager.SetDifficulty(SessionHandler.CurrentDifficulty);
                };

                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("MESSAGE", new string[] { "Settings might change to reflect server enforcements" }, toDo));

                return false;
            }

            return true;
        }
    }
}
