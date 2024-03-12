using HarmonyLib;
using RimWorld;
using Shared;
using System;
using UnityEngine;
using Verse;

namespace GameClient
{
    [HarmonyPatch(typeof(MainMenuDrawer), "DoMainMenuControls")]
    public static class SaveMenuPatch
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.isConnectedToServer && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 2, buttonSize.x, buttonSize.y), ""))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.SyncingSaveWithTheServer".Translate()));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.ToggleDisconnecting(true);
                    SaveManager.ForceSave();
                }

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 7) * 3, buttonSize.x, buttonSize.y), ""))
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.SyncingSaveWithTheServer".Translate()));

                    Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
                    ClientValues.ToggleQuiting(true);
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
            if (Network.isConnectedToServer && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 6) * 6, buttonSize.x, buttonSize.y), ""))
                {
                    if (!Network.isConnectedToServer) DialogManager.PushNewDialog(new RT_Dialog_Error("RimworldTogether.PlayerServerPresenceRequest".Translate()));
                    else
                    {
                        Find.MainTabsRoot.EscapeCurrentTab(playSound: false);

                        Action r1 = delegate
                        {
                            DialogManager.PushNewDialog(new RT_Dialog_Wait("RimworldTogether.WaitingRequest".Translate()));

                            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ResetSavePacket));
                            Network.listener.EnqueuePacket(packet);
                        };

                        RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RimworldTogether.ResetSaveConfirmation".Translate(), r1, null);
                        DialogManager.PushNewDialog(d1);
                    }
                }
            }

            return true;
        }

        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.isConnectedToServer && Current.ProgramState == ProgramState.Playing)
            {
                Vector2 buttonSize = new Vector2(170f, 45f);

                if (Widgets.ButtonText(new Rect(0, (buttonSize.y + 6) * 6, buttonSize.x, buttonSize.y), "RimworldTogether.ResetSave".Translate()))
                {

                }
            }

            return;
        }
    }
}