using System;
using System.Net;
using GameClient.Misc;
using TCPNetwork.Packets;
using Shared;
using Steamworks;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public class ServerBrowserManager
    {
        private static WebClient Client { get; set; } = new WebClient();

        private static string MasterServer { get; set; } = "https://rimworldtogether.eragon.dev";

        public static ServerInfo[] GetAllServersAvailable()
        {
            try
            {
                Client.Headers.Clear();
                Client.Headers.Add("action", "Server-Infos");
                string response = Client.DownloadString(MasterServer);
                if (string.IsNullOrEmpty(response))
                {
                    Printer.Warning($"response was null");
                    return null;
                }
                AllServersPacket data = Serializer.SerializeFromString<AllServersPacket>(response);

                Printer.Warning(data, LogImportanceMode.Extreme);

                return data._serverInfos;
            }
            catch (Exception ex)
            {
                Printer.Error($"Error while trying to fetch info from the server browser.\n{ex}");
                return null;
            }
        }

        public static bool DownloadMod(ulong steamId)
        {
            try
            {
                SteamUGC.SubscribeItem(new PublishedFileId_t(steamId));
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
