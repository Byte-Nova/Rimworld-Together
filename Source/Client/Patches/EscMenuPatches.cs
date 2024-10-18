using HarmonyLib;
using RimWorld;
using Shared;
using System;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public static class SaveMenuPatch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Connected && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 2, buttonSize.x, buttonSize.y), ""))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Syncing save with the server"));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToMenu);
                    SaveManager.ForceSave();
                }

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 3, buttonSize.x, buttonSize.y), ""))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Syncing save with the server"));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToOS);
                    SaveManager.ForceSave();
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public static class AdminMenuPatch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Connected && Current.ProgramState == ProgramState.Playing && ServerValues.isAdmin && Prefs.DevMode)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 4, buttonSize.x, buttonSize.y), ""))
                {
                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    AdminMenuManager.ShowAdminMenu();
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == ClientNetworkState.Connected && Current.ProgramState == ProgramState.Playing && ServerValues.isAdmin && Prefs.DevMode)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);
                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 4, buttonSize.x, buttonSize.y), "Admin menu")) { }
            }

            return;
        }
    }

    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public static class RestartGamePatch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == ClientNetworkState.Connected && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 6) * 6, buttonSize.x, buttonSize.y), ""))
                {
                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);

                    Action r1 = delegate
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));
                        SaveManager.RequestResetSave();
                    };

                    RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("Are you sure you want to delete your save?", r1, null);
                    DialogManager.PushNewDialog(d1);
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == ClientNetworkState.Connected && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                GUI.color = Color.red;
                //Add 6 instead of 7 to prevent the button going through the UI border
                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 6) * 6, buttonSize.x, buttonSize.y), "Delete Save")) { }
                GUI.color = Color.white;
            }

            return;
        }
    }
}