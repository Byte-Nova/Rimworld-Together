using HarmonyLib;
using RimWorld;
using Shared;
using System.Collections.Generic;
using System;
using System.Linq;
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

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.isConnectedToServer || Network.isTryingToConnect) return true;

                        SetupQuickConnectVariables();

                        bool isInvalid = false;
                        if (string.IsNullOrWhiteSpace(Network.ip)) isInvalid = true;
                        if (string.IsNullOrWhiteSpace(Network.port)) isInvalid = true;
                        if (string.IsNullOrWhiteSpace(ClientValues.username)) isInvalid = true;

                        if (isInvalid) DialogManager.PushNewDialog(new RT_Dialog_OK("You must join a server first to use this feature!"));
                        else ShowQuickConnectFloatMenu();
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
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "Play Together"))
                    {

                    }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "▶"))
                    {

                    }
                }
            }

            private static void SetupQuickConnectVariables()
            {
                string[] details = PreferenceManager.LoadConnectionDetails();
                Network.ip = details[0];
                Network.port = details[1];

                details = PreferenceManager.LoadLoginDetails();
                ClientValues.username = details[0];
            }

            private static void ShowQuickConnectFloatMenu()
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<Tuple<string, int>> quickConnectTuples = new List<Tuple<string, int>>()
                {
                    Tuple.Create($"Join '{Network.ip}:{Network.port}' as '{ClientValues.username}'", 0),
                };

                foreach (Tuple<string, int> tuple in quickConnectTuples)
                {
                    FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                    {
                        ClientValues.ToggleQuickConnecting(true);

                        DialogManager.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                        Network.StartConnection();

                        string[] details = PreferenceManager.LoadLoginDetails();
                        JoinDetailsJSON loginDetails = new JoinDetailsJSON();
                        loginDetails.username = details[0];
                        loginDetails.password = Hasher.GetHashFromString(details[1]);
                        loginDetails.clientVersion = CommonValues.executableVersion;
                        loginDetails.runningMods = ModManager.GetRunningModList().ToList();

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.LoginClientPacket), loginDetails);
                        Network.listener.EnqueuePacket(packet);
                    });

                    list.Add(item);
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
    }
}