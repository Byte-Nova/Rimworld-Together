using HarmonyLib;
using RimWorld;
using Shared;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;
using GameClient.Core.Preferences;
using GameClient.Managers;
using GameClient.TCP;
using GameClient.Misc;

namespace GameClient.Patches.Pages
{
    public class MainMenuPatches
    {
        [HarmonyPatch(typeof(VersionControl), nameof(VersionControl.DrawInfoInCorner))]
        private static class VersionControl_DrawInfoInCorner_Patch
        {
            private static void Postfix()
            {
                string toDisplay = $"RimWorld Together v{CommonValues.executableVersion}";
                Vector2 size = Text.CalcSize(toDisplay);
                Rect rect = new Rect(10f, 73f, size.x, size.y);

                Text.Font = GameFont.Small;

                GUI.color = Color.white.ToTransparent(0.5f);
                Widgets.Label(rect, toDisplay);
                GUI.color = Color.white;
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
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y + 0.5f);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.state != ClientNetworkState.Disconnected) return true;
                        else if (!UserLoginManagerH.CheckIfLoginIsValid()) UserLoginManager.PromptCreateAccount(false);
                        else ConnectionManager.ShowConnectDialogs();
                    }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.state != ClientNetworkState.Disconnected) return true;
                        else if (!UserLoginManagerH.CheckIfLoginIsValid()) UserLoginManager.PromptCreateAccount(true);
                        else UserLoginManager.QuickConnectUser();
                    }
                }

                return true;
            }

            [HarmonyPostfix]
            public static void DoPost(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y + 0.5f);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Play Together")) { }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "▶")) { }
                }
            }
        }
    }
}