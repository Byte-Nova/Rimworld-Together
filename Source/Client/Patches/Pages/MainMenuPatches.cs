using HarmonyLib;
using RimWorld;
using Shared;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
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
                        ConnectionManager.ShowConnectDialogs();
                    }

                    buttonSize = new Vector2(45f, 45f);
                    buttonLocation = new Vector2(rect.x - 50f, rect.y);
                    if (Widgets.ButtonText(new Rect(buttonLocation.x, buttonLocation.y, buttonSize.x, buttonSize.y), ""))
                    {
                        if (Network.state != ClientNetworkState.Disconnected) return true;

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
                    Vector2 buttonLocation = new Vector2(rect.x, rect.y + 0.5f);
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
                ConnectionDataFile connectionData = PreferenceManager.LoadConnectionData();
                Network.ip = connectionData.IP;
                Network.port = connectionData.Port;

                LoginDataFile loginData = PreferenceManager.LoadLoginData();
                ClientValues.username = loginData.Username;
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

                        if (Network.state == ClientNetworkState.Connected)
                        {
                            LoginDataFile loginData = PreferenceManager.LoadLoginData();

                            LoginData data = new LoginData();
                            data._username = loginData.Username;
                            data._password = Hasher.GetHashFromString(loginData.Password);
                            data._version = CommonValues.executableVersion;
                            data._runningMods = ModManagerHelper.GetRunningModList();

                            Packet packet = Packet.CreatePacketFromObject(nameof(LoginManager), data);
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