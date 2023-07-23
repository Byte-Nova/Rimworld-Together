using System.Globalization;
using System.IO;
using RimworldTogether.GameClient.Files;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Values;
using UnityEngine;
using Verse;

namespace RimworldTogether.GameClient.Core
{
    public class Main
    {
        public static Master master = new Master();
        public static ModConfigs modConfigs = new ModConfigs();

        public static string mainPath;

        public static string modPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string savesPath;

        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether() 
            {
                PrepareCulture();
                PreparePaths();
                LoadClientPreferences();
            }

            private static void PrepareCulture()
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
                CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
            }

            public static void PreparePaths()
            {
                mainPath = Application.persistentDataPath;
                modPath = Path.Combine(mainPath, "Rimworld Together");
                connectionDataPath = Path.Combine(modPath, "ConnectionData.json");
                loginDataPath = Path.Combine(modPath, "LoginData.json");
                clientPreferencesPath = Path.Combine(modPath, "Preferences.json");
                savesPath = Path.Combine(mainPath, "Saves");

                if (!Directory.Exists(modPath)) Directory.CreateDirectory(modPath);
            }

            private static void LoadClientPreferences()
            {
                ClientPreferencesFile newPreferences;

                if (File.Exists(clientPreferencesPath))
                {
                    newPreferences = Serializer.SerializeFromFile<ClientPreferencesFile>(clientPreferencesPath);
                    ClientValues.autosaveDays = int.Parse(newPreferences.AutosaveInterval);
                    ClientValues.autosaveInternalTicks = Mathf.RoundToInt(ClientValues.autosaveDays * 60000f);
                }

                else
                {
                    ClientValues.autosaveDays = 3;
                    ClientValues.autosaveInternalTicks = 60000f;

                    Saver.SaveClientPreferences(ClientValues.autosaveDays.ToString());
                }
            }
        }
    }
}