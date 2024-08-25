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
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y + 0.5f);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.state != NetworkState.Disconnected) return true;
                        DialogShortcuts.ShowConnectDialogs();
                    }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.state != NetworkState.Disconnected) return true;

                        SetupQuickConnectVariables();

                        bool isInvalid = false;
                        if (string.IsNullOrWhiteSpace(Network.ip)) isInvalid = true;
                        if (string.IsNullOrWhiteSpace(Network.port)) isInvalid = true;
                        if (string.IsNullOrWhiteSpace(ClientValues.username)) isInvalid = true;

                        if (isInvalid) DialogManager.PushNewDialog(new RT_Dialog_OK("RTFastJoinUnAvailable".Translate()));
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
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y + 0.5f);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), "RTMainMenuButton".Translate()))
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
                string[] details = PreferenceManager.LoadConnectionData();
                Network.ip = details[0];
                Network.port = details[1];

                details = PreferenceManager.LoadLoginData();
                ClientValues.username = details[0];
            }

            private static void ShowQuickConnectFloatMenu()
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<Tuple<string, int>> quickConnectTuples = new List<Tuple<string, int>>()
                {
                    Tuple.Create("RTFastJoin".Translate(Network.ip, Network.port, ClientValues.username), 0),
                };

                foreach (Tuple<string, int> tuple in quickConnectTuples)
                {
                    FloatMenuOption item = new FloatMenuOption(tuple.Item1, delegate
                    {
                        ClientValues.ToggleQuickConnecting(true);

                        DialogManager.PushNewDialog(new RT_Dialog_Wait("RTDialogTryingToConnect".Translate()));
                        Network.StartConnection();

                        if (Network.state == NetworkState.Connected)
                        {
                            string[] details = PreferenceManager.LoadLoginData();
                            LoginData loginData = new LoginData();
                            loginData.username = details[0];
                            loginData.password = Hasher.GetHashFromString(details[1]);
                            loginData.clientVersion = CommonValues.executableVersion;
                            loginData.runningMods = ModManager.GetRunningModList().ToList();

                            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.LoginClientPacket), loginData);
                            Network.listener.EnqueuePacket(packet);
                        }
                    });

                    list.Add(item);
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
    }
}