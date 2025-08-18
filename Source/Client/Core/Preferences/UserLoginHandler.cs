using System;
using System.Collections.Generic;
using System.IO;
using GameClient.Dialogs;
using GameClient.Files;
using GameClient.Managers;
using GameClient.Misc;
using GameClient.Values;
using TCPNetwork.Packets;
using Shared;
using UnityEngine;
using Verse;

namespace GameClient.Core.Preferences
{
    public static class UserLoginHandler
    {
        public static void SaveLoginData(LoginDataFile file) { Serializer.SerializeToFile(Master.LoginDataPath, file); }

        public static LoginDataFile LoadLoginData()
        {
            RemoveOnNextUpdate.FixSpacesInUsername();

            // For testing purposes

            if (Input.GetKey(KeyCode.LeftShift)) return GetTestingLoginFile();
            else
            {
                if (File.Exists(Master.LoginDataPath)) return Serializer.SerializeFromFile<LoginDataFile>(Master.LoginDataPath);
                else return new LoginDataFile();
            }
        }

        public static void DeleteLoginData() { File.Delete(Master.LoginDataPath); }

        public static void UseLoginData()
        {
            if (SessionValues.CurrentNetworkState != CommonEnumerators.ClientNetworkState.Connected) return;
            else
            {
                LoginDataFile file = LoadLoginData();
                ClientValues.Username = file.Username;
                ClientValues.Uid = file.UID;

                LoginData data = new LoginData();
                data._uid = file.UID;
                data._username = file.Username;
                data._runningMods = ModManagerH.GetRunningModList();

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.LoginManager, data);
            }
        }

        private static LoginDataFile GetTestingLoginFile()
        {
            LoginDataFile file = new LoginDataFile();
            file.UID = "UID";
            file.Username = "Username";
            return file;
        }

        public static void PromptCreateAccount(bool isQuickConnect)
        {
            Action toDo = delegate
            {
                if (!StringChecker.CheckIfStringValid(RT_Dialog_Inputs.DialogInputResults[0]))
                {
                    RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR",
                        new string[] { "Your username contains illegal characters", "Please choose another one and try again" }));
                }

                else
                {
                    AssignPlayerUsername(RT_Dialog_Inputs.DialogInputResults[0]);
                    AssignPlayerHash();

                    if (isQuickConnect) QuickConnectUser();
                    else ConnectionManager.ShowConnectDialogs();
                }
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Inputs("Question", 
                new string[] { "What would you like your username to be?" }, new bool[] { false }, toDo));
        }

        public static void AssignPlayerUsername(string user)
        {
            LoginDataFile file = LoadLoginData();
            file.Username = user;
            SaveLoginData(file);
        }

        public static void AssignPlayerHash()
        {
            if (UserLoginManagerH.CheckIfLoginIsValid()) return;
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);

                LoginDataFile file = LoadLoginData();
                file.UID = Hasher.GetHashFromString(timeSpan.TotalMilliseconds).Substring(0, 16);
                SaveLoginData(file);
            }
        }

        public static void QuickConnectUser()
        {
            UserLoginManagerH.SetupQuickConnectVariables();
            if (UserLoginManagerH.CheckIfQuickConnectIsValid()) UserLoginManagerH.ShowQuickConnectFloatMenu();
            else RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You must join a server first to use this feature!" }));
        }
    }

    public static class UserLoginManagerH
    {
        public static bool CheckIfLoginIsValid()
        {
            LoginDataFile file = UserLoginHandler.LoadLoginData();
            if (string.IsNullOrWhiteSpace(file.UID)) return false;
            else if (string.IsNullOrWhiteSpace(file.Username)) return false;
            else return true;
        }

        public static void SetupQuickConnectVariables()
        {
            ConnectionDataFile connectionData = ConnectionDataHandler.LoadConnectionData();
            ClientNetwork.Ip = connectionData.IP;
            ClientNetwork.Port = connectionData.Port;
        }

        public static void ShowQuickConnectFloatMenu()
        {
            List<Tuple<string, int>> quickConnectTuples = new List<Tuple<string, int>>()
            {
                Tuple.Create($"Join latest server > {ClientNetwork.Ip}:{ClientNetwork.Port}", 0),
            };

            FloatMenuOption tuple1 = new FloatMenuOption(quickConnectTuples[0].Item1, delegate
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Trying to connect to server"));
                ClientNetwork _ = new ClientNetwork();
            });

            Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>() { tuple1 }));
        }

        public static bool CheckIfQuickConnectIsValid()
        {
            if (string.IsNullOrWhiteSpace(ClientNetwork.Ip)) return false;
            else if (string.IsNullOrWhiteSpace(ClientNetwork.Port)) return false;
            else return true;
        }
    }
}
