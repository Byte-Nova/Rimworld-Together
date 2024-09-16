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
                        //if (Network.state != ClientNetworkState.Disconnected) return true;
                        //DialogShortcuts.ShowConnectDialogs();

                        Action toDo = delegate
                        {
                            ModConfigData data = new ModConfigData();
                            data._stepMode = ModConfigStepMode.Send;
                            data._configFile = new ModConfigFile();
                            data._configFile.Mods = DialogManager.dialogTupleListingResultString;
                            data._configFile.Categories = DialogManager.dialogTupleListingResultInt;

                            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ModPacket), data);
                            Network.listener.EnqueuePacket(packet);
                        };

                        string[] loadedMods = ModManager.GetRunningModList();
                        RT_Dialog_ListingWithTuple dialog = new RT_Dialog_ListingWithTuple("Mod Manager" , "Manage mods for the server", loadedMods, toDo);
                        DialogManager.PushNewDialog(dialog);
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
                            data._runningMods = ModManager.GetRunningModList();

                            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.LoginClientPacket), data);
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