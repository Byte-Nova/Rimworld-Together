using System.IO;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Files;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameClient.Misc
{
    public static class Saver
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

        public static void SaveLoginDetails(string username, string password)
        {
            LoginDataFile newLoginData;
            if (File.Exists(Main.loginDataPath)) newLoginData = Serializer.SerializeFromFile<LoginDataFile>(Main.loginDataPath);
            else newLoginData = new LoginDataFile();

            newLoginData.username = username;
            newLoginData.password = password;

            Serializer.SerializeToFile(Main.loginDataPath, newLoginData);
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