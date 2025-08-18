using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;
using GameClient.Core.Preferences;
using GameClient.Managers;
using System.Collections.Generic;
using System.Linq;
using GameClient.Values;

namespace GameClient.Patches.Pages
{
    [HarmonyPatch(typeof(VersionControl), nameof(VersionControl.DrawInfoInCorner))]
    public static class VersionControl_DrawInfoInCorner_Patch
    {
        public static void Postfix()
        {
            string toDisplay = $"RimWorld Together V-{CommonValues.ExecutableVersion}";
            Vector2 size = Text.CalcSize(toDisplay);
            Rect rect = new Rect(10f, 73f, size.x, size.y);

            Text.Font = GameFont.Small;

            GUI.color = Color.white.ToTransparent(0.5f);
            Widgets.Label(rect, toDisplay);
            GUI.color = Color.white;
        }
    }

    [HarmonyPatch(typeof(Verse.OptionListingUtility), nameof(Verse.OptionListingUtility.DrawOptionListing))]
    public static class Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Rect rect, List<ListableOption> optList)
        {
            if (Current.ProgramState != ProgramState.Entry) return true;
            else
            {
                if (optList.First().GetType() == typeof(ListableOption))
                {
                    optList.Insert(0, new ListableOption("Play Together", delegate
                    {
                        if (SessionValues.CurrentNetworkState != ClientNetworkState.Disconnected) return;
                        else if (!UserLoginManagerH.CheckIfLoginIsValid()) UserLoginHandler.PromptCreateAccount(false);
                        else ConnectionManager.ShowWelcomeDialogs();
                    }));
                }

                return true;
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public static class PatchButton
    {
        [HarmonyPrefix]
        public static bool DoPre(Rect rect)
        {
            if (Current.ProgramState == ProgramState.Entry)
            {
                Vector2 buttonSize = new Vector2(45f, 45f);
                Vector2 buttonLocation = new Vector2(rect.x - 50f, rect.y);
                if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                {
                    if (SessionValues.CurrentNetworkState != ClientNetworkState.Disconnected) return true;
                    else if (!UserLoginManagerH.CheckIfLoginIsValid()) UserLoginHandler.PromptCreateAccount(true);
                    else UserLoginHandler.QuickConnectUser();
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost(Rect rect)
        {
            if (Current.ProgramState == ProgramState.Entry)
            {
                Vector2 buttonSize = new Vector2(45f, 45f);
                Vector2 buttonLocation = new Vector2(rect.x - 50f, rect.y);
                if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "▶")) { }
            }
        }
    }
}