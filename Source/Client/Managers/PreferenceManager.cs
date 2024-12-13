using Shared;
using System.IO;
using UnityEngine;

namespace GameClient
{
    //Class that handles saving and loading client preferences into the game

    public static class PreferenceManager
    {
        //Saves the connection data

        public static void SaveConnectionData(string ip, string port)
        {
            ConnectionDataFile newConnectionData;
            if (File.Exists(Master.connectionDataPath)) newConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else newConnectionData = new ConnectionDataFile();

            newConnectionData.IP = ip;
            newConnectionData.Port = port;

            Serializer.SerializeToFile(Master.connectionDataPath, newConnectionData);
        }

        //Loads the connection data

        public static ConnectionDataFile LoadConnectionData()
        {
            if (File.Exists(Master.connectionDataPath)) return Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else return new ConnectionDataFile();
        }

        //Saves the login data

        public static void SaveLoginData(string username, string password)
        {
            LoginDataFile newLoginData;
            if (File.Exists(Master.loginDataPath)) newLoginData = Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
            else newLoginData = new LoginDataFile();

            newLoginData.Username = username;
            newLoginData.Password = password;

            Serializer.SerializeToFile(Master.loginDataPath, newLoginData);
        }

        //Loads the login data

        public static LoginDataFile LoadLoginData()
        {
            if (File.Exists(Master.loginDataPath)) return Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
            else return new LoginDataFile();
        }

        //Saves the client preferences

        public static void SaveClientPreferences()
        {
            ClientPreferencesFile newClientPreferences;
            if (File.Exists(Master.clientPreferencesPath)) newClientPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
            else newClientPreferences = new ClientPreferencesFile();

            newClientPreferences.AutosaveInterval = ClientValues.autosaveDays.ToString();
            newClientPreferences.VerboseMode = (int)ClientValues.currentVerboseMode;

            Serializer.SerializeToFile(Master.clientPreferencesPath, newClientPreferences);
        }

        //Loads client preferences

        public static void LoadClientPreferences()
        {
            ClientPreferencesFile newPreferences;

            if (File.Exists(Master.clientPreferencesPath))
            {
                newPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
                ClientValues.autosaveDays = float.Parse(newPreferences.AutosaveInterval);
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
                ClientValues.currentVerboseMode = (ClientValues.VerboseMode)newPreferences.VerboseMode;
            }

            else
            {
                ClientValues.autosaveDays = 3;
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
                ClientValues.currentVerboseMode = ClientValues.VerboseMode.None;

                SaveClientPreferences();
            }
        }
    }
}