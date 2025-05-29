using System;
using System.Collections.Generic;
using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    public static class LoginManager
    {
        [HandlesPacket(PacketHeader.LoginManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            LoginData data = Serializer.ConvertBytesToObject<LoginData>(bytes);
            HandleUser(client, data);
        }

        // flow control (needed hardly)
        public static void HandleUser(ServerClient client, LoginData data)
        {
            if (!UserManagerH.CheckLoginData(client, data)) return;

            if (UserManagerH.CheckIfUserExists(client, data))
                LoginUser(client, data);
            else
                RegisterUser(client, data);
        }

        public static void LoginUser(ServerClient client, LoginData data)
        {
            if (!UserManagerH.CheckIfUserAuthCorrect(client, data)) return;

            client.UserFile.SetLoginDetails(data);
            client.LoadUserFromFile();

            if (UserManagerH.CheckIfUserBanned(client)) return;
            if (!UserManagerH.CheckWhitelist(client)) return;
            if (WorldManager.CheckIfWorldExists() && ModManager.CheckIfModConflict(client, data)) return;

            LoginManagerH.RemoveOldClientSessions(client);
            InformationDisplayer.DisplayLogin(client);

            PostLogin(client);
        }

        public static void RegisterUser(ServerClient client, LoginData data)
        {
            try
            {
                client.UserFile.SetLoginDetails(data);
                UserManagerH.SaveUserFile(client.UserFile);
                InformationDisplayer.DisplayRegister(client);
                LoginUser(client, data);
            }
            catch
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.RegisterError);
            }
        }

        private static void PostLogin(ServerClient client)
        {
            SiteManager.SetSiteInfoForClient(client);
            UserManager.SendPlayerRecount();
            GlobalDataManager.SendServerGlobalData(client);

            // MOTD + default join lines
            foreach (string str in ChatManager.DefaultJoinMessages)
                ChatManager.SendConsoleMessage(client, str);

            if (Master.ChatConfig.EnableMoTD)
                ChatManager.SendServerMessage(client, $"MoTD > {Master.ChatConfig.MessageOfTheDay}");

            // in-game join notification
            if (Master.ChatConfig.LoginNotifications)
                ChatManager.BroadcastServerNotification($"{client.UserFile.Label} has joined the server!");

            // console / Discord notification (extra feature you added)
            if (Master.DiscordConfig?.Enabled == true)
            {
                int count = NetworkHelper.GetConnectedClientsSafe().Length;
                _ = DiscordManager.SendConsoleMessageAsync(
                    $"[Connect] {client.UserFile.Label} (UID: {client.UserFile.Uid}) has joined the server!. Players: {count}"
                );
            }

            if (WorldManager.CheckIfWorldExists())
            {
                if (SaveManager.CheckIfUserHasSave(client))
                    SaveSenderManager.SendSaveToClient(client);
                else
                    WorldManagerSender.SendWorld(client);
            }
            else
                WorldManager.RequireWorldFile(client);
        }
    }

    public static class LoginManagerH
    {
        public static void RemoveOldClientSessions(ServerClient client)
        {
            foreach (ServerClient other in NetworkHelper.GetConnectedClientsSafe())
            {
                if (other == client) continue;
                if (other.UserFile.Uid == client.UserFile.Uid)
                    DenyConnectionWithReason(other, LoginResponse.ExtraLogin);
            }
        }

        public static void DenyConnectionWithReason(ServerClient client,
                                                    LoginResponse response,
                                                    object? extra = null)
        {
            LoginData login = new LoginData { _tryResponse = response };
            if (response == LoginResponse.WrongMods)
                login._extraDetails = (List<string>)extra!;
            else if (response == LoginResponse.WrongVersion)
                login._extraDetails = new List<string> { CommonValues.ExecutableVersion };

            client.Listener.EnqueuePacket(PacketHeader.LoginManager, login);
            client.Listener.DisconnectFlag = true;
        }
    }
}