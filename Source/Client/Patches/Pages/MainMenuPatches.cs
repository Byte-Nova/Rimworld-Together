﻿using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GameClient
{
    public class MainMenuPatches
    {
        [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
        public static class PatchButton
        {
            [HarmonyPrefix]
            public static bool DoPre(Rect rect)
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    Vector2 buttonSize = new Vector2(170f, 45f);
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.isConnectedToServer || Network.isTryingToConnect) return true;
                        else DialogShortcuts.ShowConnectDialogs();
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
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "RimworldTogether.PlayTogether".Translate()))
                    {

                    }
                }
            }
        }
    }
}