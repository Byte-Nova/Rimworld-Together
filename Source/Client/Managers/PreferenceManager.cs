using System.IO;
using System.Collections.Generic;
using Shared;

namespace GameClient
{
    public static class PreferenceManager
    {
        public static void SaveConnectionDetails(string ip, string port)
        {
            ConnectionDataFile newConnectionData;
            if (File.Exists(Master.connectionDataPath)) newConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else newConnectionData = new ConnectionDataFile();

            newConnectionData.ip = ip;
            newConnectionData.port = port;

            Serializer.SerializeToFile(Master.connectionDataPath, newConnectionData);
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

            if (File.Exists(Master.connectionDataPath))
            {
                ConnectionDataFile previousConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
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
            if (File.Exists(Master.loginDataPath)) newLoginData = Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
            else newLoginData = new LoginDataFile();

            newLoginData.username = username;
            newLoginData.password = password;

            Serializer.SerializeToFile(Master.loginDataPath, newLoginData);
        }

        public static void FetchLoginDetails()
        {
            if (File.Exists(Master.loginDataPath))
            {
                LoginDataFile previousLoginData = Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { previousLoginData.username, previousLoginData.password });
            }
            else DialogManager.currentDialogInputs.SubstituteInputs(new List<object>() { "", "" });
        }

        public static void SaveClientPreferences(string autosaveInterval)
        {
            ClientPreferencesFile newClientPreferences;
            if (File.Exists(Master.clientPreferencesPath)) newClientPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
            else newClientPreferences = new ClientPreferencesFile();

            newClientPreferences.AutosaveInterval = autosaveInterval;

            Serializer.SerializeToFile(Master.clientPreferencesPath, newClientPreferences);
        }
    }
}