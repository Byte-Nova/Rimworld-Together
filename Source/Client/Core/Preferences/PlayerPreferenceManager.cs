using System.IO;
using GameClient.Files;
using GameClient.Values;
using Shared;
using UnityEngine;

namespace GameClient.Core.Preferences
{
    public static class PlayerPreferenceManager
    {
        public static void SavePlayerPreferences()
        {
            ClientPreferencesFile newClientPreferences;
            if (File.Exists(Master.clientPreferencesPath)) newClientPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(Master.clientPreferencesPath);
            else newClientPreferences = new ClientPreferencesFile();

            newClientPreferences.AutosaveInterval = ClientValues.autosaveDays.ToString();
            newClientPreferences.VerboseMode = (int)ClientValues.currentVerboseMode;

            Serializer.SerializeToFile(Master.clientPreferencesPath, newClientPreferences);
        }

        public static void LoadPlayerPreferences()
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

                SavePlayerPreferences();
            }
        }
    }
}
