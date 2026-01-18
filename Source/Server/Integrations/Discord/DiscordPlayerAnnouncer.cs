using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace GameServer.Integrations.Discord
{
    public static class DiscordPlayerAnnouncer
    {
        private static readonly object LockObj = new object();
        private static readonly HashSet<string> JoinedUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void AnnounceFullyJoined(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return;

            bool shouldSend = false;

            lock (LockObj)
            {
                if (!JoinedUsers.Contains(username))
                {
                    JoinedUsers.Add(username);
                    shouldSend = true;
                }
            }

            if (!shouldSend)
                return;

            string msg = $"SERVER: {username} has joined the server!";

            DiscordBridge.TryRelayServerConsoleLine(msg, LogMode.Title);
            DiscordBridge.TryRelayServerNoticeToDiscordChat(msg);
        }

        public static void AnnounceLeft(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return;

            lock (LockObj)
            {
                JoinedUsers.Remove(username);
            }

            string msg = $"SERVER: {username} has left the server!";

            DiscordBridge.TryRelayServerConsoleLine(msg, LogMode.Warning);
            DiscordBridge.TryRelayServerNoticeToDiscordChat(msg);
        }

        public static void Clear()
        {
            lock (LockObj)
            {
                JoinedUsers.Clear();
            }
        }
    }
}