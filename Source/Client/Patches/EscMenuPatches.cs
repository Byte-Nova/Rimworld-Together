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
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RTDialogSync".Translate()));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToMenu);
                    SaveManager.ForceSave();
                }

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 3, buttonSize.x, buttonSize.y), ""))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RTDialogSync".Translate()));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.SetIntentionalDisconnect(true, DisconnectionManager.DCReason.SaveQuitToOS);
                    SaveManager.ForceSave();
                }
            }

            return true;
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
                    if (Network.state == ClientNetworkState.Disconnected) DialogManager.PushNewDialog(new RT_Dialog_Error("RTDialogNeedInServer".Translate()));
                    else
                    {
                        Find.MainTabsRoot.EscapeCurrentTab(playSound: false);

                        Action r1 = delegate
                        {
                            DialogManager.PushNewDialog(new RT_Dialog_Wait("RTDialogServerWait".Translate()));

                            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ResetSavePacket));
                            Network.listener.EnqueuePacket(packet);
                        };

                        RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTDialogDeleteConfirm".Translate(), r1, null);
                        DialogManager.PushNewDialog(d1);
                    }
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

                GUI.color = new Color(1f, 0.3f, 0.35f);
                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 6) * 6, buttonSize.x, buttonSize.y), "RTDialogDeleteSave".Translate()))
                {

                }
                GUI.color = Color.white;
            }

            return;
        }
    }
}