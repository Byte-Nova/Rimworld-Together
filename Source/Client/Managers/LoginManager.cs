using GameClient.Dialogs;
using GameClient.Files;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using Shared;
using System;
using System.Collections.Generic;
using TCPNetwork.Packets;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class LoginManager
    {
        [HandlesPacket(PacketHeader.LoginManager)]
        private static void ParsePacket(byte[] bytes)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(bytes);

            switch (data._tryResponse)
            {
                case LoginResponse.Invalid:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Login details are invalid!", "Please try again or reset your account!" }));
                    break;

                case LoginResponse.Ban:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You are banned from this server!" }));
                    break;

                case LoginResponse.Duplicate:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You connected from another place!" }));
                    break;

                case LoginResponse.Mods:
                    ModManagerH.GetConflictingMods(data);
                    break;

                case LoginResponse.Full:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Server is full!" }));
                    break;

                case LoginResponse.Whitelist:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Server is whitelisted!" }));
                    break;

                case LoginResponse.Version:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { $"Mod version mismatch! Expected version '{data._extraDetails[0]}'" }));
                    break;

                case LoginResponse.NoWorld:
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { $"Server is currently being set up! Join again later!" }));
                    break;
            }
        }

        public static void UseLoginData()
        {
            if (SessionHandler.CurrentNetworkState != CommonEnumerators.ClientNetworkState.Connected) return;
            else
            {
                LoginData data = new LoginData();

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    data._username = "Test";
                    data._password = "1234";
                }

                else
                {
                    PersistentSettings settings = PersistentSettings.Load();
                    data._username = settings.UserSettings.Username;
                    data._password = settings.UserSettings.Password;
                }

                SessionHandler.Username = data._username;
                data._runningMods = ModManagerH.GetRunningModList();
                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.LoginManager, data);
            }
        }

        public static void PromptCreateAccount(bool isQuickConnect)
        {
            Action toDo = delegate
            {
                bool isInvalid = false;
                if (!StringChecker.CheckIfStringValid(RT_Dialog_Inputs.DialogInputResults[0])) isInvalid = true;
                else if (!StringChecker.CheckIfStringValid(RT_Dialog_Inputs.DialogInputResults[1])) isInvalid = true;

                if (isInvalid)
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR",
                        new string[] { "Your login details contains illegal characters", "Please try again" }));
                }

                else
                {
                    PersistentSettings settings = PersistentSettings.Load();
                    settings.UserSettings.Set(RT_Dialog_Inputs.DialogInputResults[0], Hasher.GetHashFromString(RT_Dialog_Inputs.DialogInputResults[1]));
                    settings.Save();

                    if (isQuickConnect) QuickConnectUser();
                    else ConnectionManager.ShowConnectDialogs();
                }
            };

            Action toDo2 = delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Inputs("Account Setup",
                    new string[] { "Username", "Password" }, new bool[] { false, true }, toDo));
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("Account Setup", new string[] { "Please create or log into your account" }, toDo2));
        }

        public static void QuickConnectUser()
        {
            PersistentSettings settings = PersistentSettings.Load();
            TCPNetwork.Network.Ip = settings.ServerSettings.LatestIP;
            TCPNetwork.Network.Port = settings.ServerSettings.LatestPort;

            if (StringChecker.CheckIfStringValid(TCPNetwork.Network.Ip) && StringChecker.CheckIfStringValid(TCPNetwork.Network.Port))
            {
                LoginManagerH.ShowQuickConnectFloatMenu();
            }

            else
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You must join a server first to use this feature!" }));
            }
        }
    }

    public static class LoginManagerH
    {
        public static bool CheckIfLoginIsValid()
        {
            PersistentSettings settings = PersistentSettings.Load();
            if (!StringChecker.CheckIfStringValid(settings.UserSettings.Username)) return false;
            else if (!StringChecker.CheckIfStringValid(settings.UserSettings.Password)) return false;
            else return true;
        }

        public static void ShowQuickConnectFloatMenu()
        {
            List<Tuple<string, int>> quickConnectTuples = new List<Tuple<string, int>>()
            {
                Tuple.Create($"Join latest server > {TCPNetwork.Network.Ip}:{TCPNetwork.Network.Port}", 0),
            };

            FloatMenuOption tuple1 = new FloatMenuOption(quickConnectTuples[0].Item1, delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                ClientNetwork _ = new ClientNetwork();
            });

            Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { tuple1 }));
        }
    }
}
