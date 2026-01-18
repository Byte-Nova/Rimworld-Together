using GameServer.Core;
using GameServer.Misc;
using TCPNetwork.Packets;
using Shared;
using static Shared.CommonEnumerators;
using TCPNetwork.Files.Client;
using Shared.Misc;
using GameServer.Integrations.Discord;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{
    public static class LoginManager
    {
        [HandlesPacket(PacketHeader.LoginManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(bytes);

            HandleUser(client, data);
        }

        public static void HandleUser(ServerClient client, LoginData data)
        {
            client.UserFile = new UserFile();
            client.UserFile.UpdateLoginDetails(data);

            if (UserManagerH.CheckIfUserExists(client, data))
            {
                var result = TryLoginUser(client, data);
                if (!string.IsNullOrEmpty(result))
                {
                    Printer.Error($"Error during login: {result}");
                }
            }
            else RegisterUser(client, data);
        }

        public static string TryLoginUser(ServerClient client, LoginData data)
        {
            if (!UserManagerH.CheckIfUserAuthCorrect(client, data))
                return $"Login details to not match, either the password or username is wrong for {data._username}";

            client.LoadUserFromFile(client);

            if (UserManagerH.CheckIfUserBanned(client)) return $"{data._username} is banned";
            if (!UserManagerH.CheckWhitelist(client)) return $"{data._username} is not whitelisted";
            if (WorldManager.CheckIfWorldExists() && ModManager.CheckIfModConflict(client, data)) return $"{data._username} has a mod conflict";

            LoginManagerH.RemoveOldClientSessions(client);

            InformationDisplayer.DisplayLogin(client);

            PostLogin(client);
            return "";
        }

        public static void RegisterUser(ServerClient client, LoginData data)
        {
            client.UserFile.UpdateHash();

            InformationDisplayer.DisplayRegister(client);

            var error = TryLoginUser(client, data);
            if (!string.IsNullOrEmpty(error))
            {
                Printer.Error($"Error during login: {error}");
            }
        }

        private static void PostLogin(ServerClient client)
        {
            SiteManager.SetSiteInfoForClient(client);

            UserManager.SendPlayerRecount();

            GlobalDataManager.SendServerGlobalData(client);

            foreach (string str in ChatManager.DefaultJoinMessages) ChatManager.SendConsoleMessage(client, str);

            if (Master.ChatConfig.EnableMoTD) ChatManager.SendServerMessage(client, $"MoTD > {Master.ChatConfig.MessageOfTheDay}");

            if (Master.ChatConfig.LoginNotifications) ChatManager.BroadcastServerNotification($"{client.UserFile.Username} has joined the server!");

            DiscordPlayerAnnouncer.AnnounceFullyJoined(client.UserFile.Username);

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client)) SaveManager.SendSaveToClient(client);
                else WorldManager.SendWorld(client);
            }
            else
            {
                Printer.Warning($"Giving first join admin permission to {client.UserFile.Username}");

                client.UserFile.UpdateAdmin(true);
                CommandData commandData = new CommandData();
                commandData._commandMode = CommandMode.Op;
                client.Listener.EnqueuePacket(PacketHeader.ConsoleManager, commandData);

                WorldManager.RequireWorldFile(client);
            }
        }
    }

    public static class LoginManagerH
    {
        public static void RemoveOldClientSessions(ServerClient client)
        {
            foreach (ServerClient toFind in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                if (toFind == client) continue;
                else
                {
                    if (toFind.UserFile.Username == client.UserFile.Username)
                    {
                        DenyConnectionWithReason(toFind, LoginResponse.Duplicate);
                    }
                }
            }
        }

        public static void DenyConnectionWithReason(ServerClient client, LoginResponse response, object extraDetails = null)
        {
            LoginData loginData = new LoginData();
            loginData._tryResponse = response;

            if (response == LoginResponse.Mods) loginData._extraDetails = (System.Collections.Generic.List<string>)extraDetails;
            else if (response == LoginResponse.Version) loginData._extraDetails = new System.Collections.Generic.List<string>() { CommonValues.ExecutableVersion };

            client.Listener.EnqueuePacket(PacketHeader.LoginManager, loginData);
            client.Listener.DisconnectSmooth();
        }
    }
}