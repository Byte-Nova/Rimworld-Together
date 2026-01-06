using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;
using GameClient.Managers;
using System.Collections.Generic;
using System.Linq;
using GameClient.Misc;

namespace GameClient.Patches.Pages
{
    [HarmonyPatchCategory("Start")]
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

    [HarmonyPatchCategory("Start")]
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
                        if (SessionHandler.CurrentNetworkState != ClientNetworkState.Disconnected) return;
                        else if (!LoginManagerH.CheckIfLoginIsValid()) LoginManager.PromptCreateAccount(false);
                        else ConnectionManager.ShowWelcomeDialogs();
                    }));
                }

                return true;
            }
        }
    }

    [HarmonyPatchCategory("Start")]
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
                    if (SessionHandler.CurrentNetworkState != ClientNetworkState.Disconnected) return true;
                    else if (!LoginManagerH.CheckIfLoginIsValid()) LoginManager.PromptCreateAccount(true);
                    else LoginManager.QuickConnectUser();
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