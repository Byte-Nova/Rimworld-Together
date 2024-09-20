using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SiteManager
    {
        //Variables

        private static readonly double taskDelayMS = 1800000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableSites)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);

            switch(siteData._stepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, siteData);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, siteData);
                    break;

                case SiteStepMode.Info:
                    SiteManagerHelper.GetSiteInfo(client, siteData);
                    break;
            }
        }

        public static void ConfirmNewSite(ServerClient client, SiteIdendity siteFile)
        {
            SiteManagerHelper.SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._siteFile = siteFile;

            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData._siteFile.Goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                Logger.Warning(((int)(siteData._siteFile.Goodwill)).ToString());
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData._stepMode = SiteStepMode.Accept;
            Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(rPacket);

            Logger.Warning($"[Created site] > {client.userFile.Username}");
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a site");
            else
            {
                SiteIdendity siteFile = new SiteIdendity();

                if (siteData._siteFile.FactionFile != null)
                {
                    FactionFile factionFile = client.userFile.FactionFile;
                    if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                        return;
                    }
                    else
                    {
                        siteFile.FactionFile = factionFile;
                    }
                }
                siteFile.Tile = siteData._siteFile.Tile;
                siteFile.Owner = client.userFile.Username;
                siteFile.Type = SiteManagerHelper.GetTypeFromDef(siteData._siteFile.Type.DefName);
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteIdendity siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._siteFile.Tile);

            if (siteFile.FactionFile != null)
            {
                if (siteFile.FactionFile.Name != client.userFile.FactionFile.Name)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData._siteFile.Tile} was attempted to be destroyed by {client.userFile.Username}, but player wasn't a part of faction {siteFile.FactionFile.Name}");
                }

                else
                {
                    FactionFile factionFile = client.userFile.FactionFile;
                    if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Member) DestroySiteFromFile(siteFile);
                    else ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                }
            }

            else
            {
                if (siteFile.Owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData._siteFile.Tile} was attempted to be destroyed by {client.userFile.Username}, but the player {siteFile.Owner} owns it");
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteIdendity siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            NetworkHelper.SendPacketToAllClients(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.Tile + SiteManagerHelper.fileExtension));
            Logger.Warning($"[Remove site] > {siteFile.Tile}");
        }

        public static async Task StartSiteTicker()
        {
            while (true)
            {
                try { SiteRewardTick(); }
                catch (Exception e) { Logger.Error($"Site tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMinutes(Master.siteValues.TimeIntervalMinute));
            }
        }

        public static void SiteRewardTick()
        {
            SiteIdendity[] sites = SiteManagerHelper.GetAllSites();

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                List<RewardFile> data = new List<RewardFile>();

                //Get player specific sites

                List<SiteIdendity> sitesToAdd = sites.ToList().FindAll(fetch => fetch.Owner == client.userFile.Username);
                sitesToAdd.AddRange(sites.ToList().FindAll(fetch => fetch.FactionFile.Name == client.userFile.FactionFile.Name));
                foreach (SiteIdendity site in sitesToAdd) data.Add(site.Type.Rewards);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SiteRewardPacket), new RewardData() {_rewardData = data.ToArray()});
                client.listener.EnqueuePacket(packet);
            }
            Logger.Warning($"[Site tick]");
        }
    }

    public static class SiteManagerHelper
    {
        public readonly static string fileExtension = ".mpsite";

        public static void SaveSite(SiteIdendity siteFile)
        {
            siteFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.Tile + fileExtension), siteFile); }
            catch (Exception e) { Logger.Error(e.ToString()); }
            
            siteFile.SavingSemaphore.Release();
        }

        public static void UpdateFaction(SiteIdendity siteFile, FactionFile toUpdateWith)
        {
            siteFile.FactionFile = toUpdateWith;
            SaveSite(siteFile);
        }

        public static SiteIdendity[] GetAllSitesFromUsername(string username)
        {
            List<SiteIdendity> sitesList = new List<SiteIdendity>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendity siteFile = Serializer.SerializeFromFile<SiteIdendity>(site);
                if (siteFile.FactionFile == null && siteFile.Owner == username) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteIdendity GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendity siteFile = Serializer.SerializeFromFile<SiteIdendity>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteIdendity siteFile = GetSiteFileFromTile(siteData._siteFile.Tile);
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }

        public static SiteIdendity[] GetAllSites()
        {
            List<SiteIdendity> sitesList = new List<SiteIdendity>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;
                sitesList.Add(Serializer.SerializeFromFile<SiteIdendity>(site));
            }

            return sitesList.ToArray();
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteIdendity siteFile = Serializer.SerializeFromFile<SiteIdendity>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static SiteConfigFile GetTypeFromDef(string defName) 
        {
            return Master.siteValues.SiteIdendityFiles.Where(S => S.DefName == defName).FirstOrDefault();
        }
    }
}
