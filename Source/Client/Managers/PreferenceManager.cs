using System.IO;
using System.Collections.Generic;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Files;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameClient.Managers
{
    public static class PreferenceManager
    {
        public static void SaveConnectionDetails(string ip, string port)
        {
            ConnectionDataFile newConnectionData;
            if (File.Exists(Main.connectionDataPath)) newConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Main.connectionDataPath);
            else newConnectionData = new ConnectionDataFile();

            newConnectionData.ip = ip;
            newConnectionData.port = port;

            Serializer.SerializeToFile(Main.connectionDataPath, newConnectionData);
        }

        public static void FetchConnectionDetails()
        {

            DialogManager.PushNewDialog(
                new RT_Dialog_2Input(
                "Connection Details",
                "IP",
                "Port",
                delegate { DialogShortcuts.ParseConnectionDetails(false); },
                DialogManager.PopDialog)
            );

            if (File.Exists(Main.connectionDataPath))
            {
                ConnectionDataFile previousConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Main.connectionDataPath);
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { previousConnectionData.ip, previousConnectionData.port });
            }

            else
            {
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { "", "" });
            }
        }

        public static void SaveLoginDetails(string username, string password)
        {
            LoginDataFile newLoginData;
            if (File.Exists(Main.loginDataPath)) newLoginData = Serializer.SerializeFromFile<LoginDataFile>(Main.loginDataPath);
            else newLoginData = new LoginDataFile();

            newLoginData.username = username;
            newLoginData.password = password;

            Serializer.SerializeToFile(Main.loginDataPath, newLoginData);
        }

        public static void FetchLoginDetails()
        {
            if (File.Exists(Main.loginDataPath))
            {
                LoginDataFile previousLoginData = Serializer.SerializeFromFile<LoginDataFile>(Main.loginDataPath);
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { previousLoginData.username, previousLoginData.password });
            }

            else
            {
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { "", "" });
            }
        }

        public static void SaveClientPreferences(string autosaveInterval)
        {
            ClientPreferencesFile newClientPreferences;
            if (File.Exists(Main.clientPreferencesPath)) newClientPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Main.clientPreferencesPath);
            else newClientPreferences = new ClientPreferencesFile();

            newClientPreferences.AutosaveInterval = autosaveInterval;

            Serializer.SerializeToFile(Main.clientPreferencesPath, newClientPreferences);
        }
    }
}