using System.IO;
using RimworldTogether.GameClient.Core;
using RimworldTogether.GameClient.Files;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameClient.Misc
{
    public static class Fetcher
    {
        public static void FetchLastConnectionDetails()
        {
            if (File.Exists(Main.connectionDataPath))
            {
                ConnectionDataFile previousConnectionData = Serializer.SerializeFromFile<ConnectionDataFile>(Main.connectionDataPath);
                DialogManager.dialog2Input.inputOneResult = previousConnectionData.ip;
                DialogManager.dialog2Input.inputTwoResult = previousConnectionData.port;
            }

            else
            {
                DialogManager.dialog2Input.inputOneResult = "";
                DialogManager.dialog2Input.inputTwoResult = "";
            }
        }

        public static void FetchLastUserDetails()
        {
            if (File.Exists(Main.loginDataPath))
            {
                LoginDataFile previousLoginData = Serializer.SerializeFromFile<LoginDataFile>(Main.loginDataPath);
                DialogManager.dialog2Input.inputOneResult = previousLoginData.username;
                DialogManager.dialog2Input.inputTwoResult = previousLoginData.password;
            }

            else
            {
                DialogManager.dialog2Input.inputOneResult = "";
                DialogManager.dialog2Input.inputTwoResult = "";
            }
        }
    }
}