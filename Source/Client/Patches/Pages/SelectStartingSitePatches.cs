﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.SceneManagement;
using Verse;

namespace GameClient
{
    public class SelectStartingSitePatches
    {
        [HarmonyPatch(typeof(Page_SelectStartingSite), "DoCustomBottomButtons")]
        public static class PathSelectStartingSitePage
        {
            [HarmonyPrefix]
            public static bool DoPre()
            {
                if (!Network.isConnectedToServer) return true;

                int num = TutorSystem.TutorialMode ? 4 : 5;
                int num2 = (num < 4 || !((float)UI.screenWidth < 540f + (float)num * (150f + 10f))) ? 1 : 2;
                int num3 = Mathf.CeilToInt((float)num / (float)num2);
                float num4 = 150f * (float)num3 + 10f * (float)(num3 + 1);
                float num5 = (float)num2 * 38f + 10f * (float)(num2 + 1);
                Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num5 - 4f, num4, num5);

                WorldInspectPane worldInspectPane = Find.WindowStack.WindowOfType<WorldInspectPane>();
                if (worldInspectPane != null && rect.x < InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f)
                {
                    rect.x = InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f;
                }

                Widgets.DrawWindowBackground(rect);

                float num6 = rect.xMin + 10f;
                float num7 = rect.yMin + 10f;
                if (Widgets.ButtonText(new Rect(num6, num7, 150f, 38f), "") || KeyBindingDefOf.Cancel.KeyDownEvent)
                {
                    SceneManager.LoadScene(0);
                    Network.listener.disconnectFlag = true;
                }
                return true;
            }

            [HarmonyPostfix]
            public static void DoPost()
            {
                if (!Network.isConnectedToServer) return;

                int num = TutorSystem.TutorialMode ? 4 : 5;
                int num2 = (num < 4 || !((float)UI.screenWidth < 540f + (float)num * (150f + 10f))) ? 1 : 2;
                int num3 = Mathf.CeilToInt((float)num / (float)num2);
                float num4 = 150f * (float)num3 + 10f * (float)(num3 + 1);
                float num5 = (float)num2 * 38f + 10f * (float)(num2 + 1);
                Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num5 - 4f, num4, num5);

                WorldInspectPane worldInspectPane = Find.WindowStack.WindowOfType<WorldInspectPane>();
                if (worldInspectPane != null && rect.x < InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f)
                {
                    rect.x = InspectPaneUtility.PaneWidthFor(worldInspectPane) + 4f;
                }

                float num6 = rect.xMin + 10f;
                float num7 = rect.yMin + 10f;
                if (Widgets.ButtonText(new Rect(num6, num7, 150f, 38f), "Disconnect") || KeyBindingDefOf.Cancel.KeyDownEvent) { }
            }
        }

        [HarmonyPatch(typeof(Page_SelectStartingSite), "PreOpen")]
        public static class PatchSettlements
        {
            [HarmonyPostfix]
            public static void DoPost()
            {
                if (!Network.isConnectedToServer) return;

                PlanetManager.BuildPlanet();
                ClientValues.ToggleReadyToPlay(true);
            }
        }
    }
}
