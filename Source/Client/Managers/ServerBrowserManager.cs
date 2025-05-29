using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using Shared;
using Steamworks;
using Verse;

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
                return Serializer.SerializeFromString<AllServersPacket>(response)._serverInfos;
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
