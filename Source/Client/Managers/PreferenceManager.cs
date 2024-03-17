using Shared;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace GameClient
{
    //Class that handles saving and loading client preferences into the game

    public static class PreferenceManager
    {
        //Saves the connection details

        public static void SaveConnectionDetails(string ip, string port)
        {
            ConnectionDataFile newConnectionData;
            if (File.Exists(Master.connectionDataPath)) newConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
            else newConnectionData = new ConnectionDataFile();

            newConnectionData.ip = ip;
            newConnectionData.port = port;

            Serializer.SerializeToFile(Master.connectionDataPath, newConnectionData);
        }

        //Loads the connection details

        public static void LoadConnectionDetails()
        {
            if (File.Exists(Master.connectionDataPath))
            {
                ConnectionDataFile previousConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Master.connectionDataPath);
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object>{ previousConnectionData.ip, previousConnectionData.port });
            }

            else
            {
                //default text fields to blank
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object> { "", "" });
            }
        }

        //Saves the login details

        public static void SaveLoginDetails(string username, string password)
        {
            LoginDataFile newLoginData;
            if (File.Exists(Master.loginDataPath)) newLoginData = Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
            else newLoginData = new LoginDataFile();

            newLoginData.username = username;
            newLoginData.password = password;

            Serializer.SerializeToFile(Master.loginDataPath, newLoginData);
        }

        //Loads the login details

        public static void LoadLoginDetails()
        {
            if (File.Exists(Master.loginDataPath))
            {
                LoginDataFile previousLoginData = Serializer.SerializeFromFile<LoginDataFile>(Master.loginDataPath);
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object> { previousLoginData.username, previousLoginData.password });
            }

            else
            {
                DialogManager.currentDialogInputs.SubstituteInputs(new List<object> { "", "" });
            }
        }

        //Saves the client preferences

        public static void SaveClientPreferences(string autosaveInterval)
        {
            ClientPreferencesFile newClientPreferences;
            if (File.Exists(Master.clientPreferencesPath)) newClientPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
            else newClientPreferences = new ClientPreferencesFile();

            newClientPreferences.AutosaveInterval = autosaveInterval;

            Serializer.SerializeToFile(Master.clientPreferencesPath, newClientPreferences);
        }

        //Loads client preferences

        public static void LoadClientPreferences()
        {
            ClientPreferencesFile newPreferences;

            if (File.Exists(Master.clientPreferencesPath))
            {
                newPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
                ClientValues.autosaveDays = int.Parse(newPreferences.AutosaveInterval);
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
            }

            else
            {
                ClientValues.autosaveDays = 3;
                ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);

                SaveClientPreferences(ClientValues.autosaveDays.ToString());
            }
        }
    }
}