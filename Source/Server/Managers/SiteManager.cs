using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using Shared.Files.Guilds;
using TCPNetwork.Files.Client;
using Shared.Files.Sites;
using TCPNetwork;
using Shared.Misc;

namespace GameServer.Managers
{
    public static class SiteManager
    {
        [HandlesPacket(PacketHeader.SiteManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.SiteAction.IsEnabled)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            SiteData data = Serializer.ConvertBytesToObject<SiteData>(bytes);

            switch (data._stepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, data);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, data);
                    break;

                case SiteStepMode.Info:
                    SiteManagerHelper.GetSiteInfo(client, data);
                    break;

                case SiteStepMode.Config:
                    ChangeUserSiteConfig(client, data);
                    break;

                case SiteStepMode.Rewards:
                    SendRewardsToPlayer(client);
                    break;

                case SiteStepMode.Worker:
                    ManageWorker(client, data);
                    break;
            }
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            siteFile.SaveSite();

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._file = siteFile;

            foreach (ServerClient cClient in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                siteData._file.Goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                cClient.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
            }

            siteData._stepMode = SiteStepMode.Accept;
            client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);

            InformationDisplayer.DisplayAddSite(siteFile.Tile.ToString());
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._file.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._file.Tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = new SiteFile();

                siteFile.Tile = siteData._file.Tile;
                siteFile.Username = client.UserFile.Username;
                siteFile.Type = SiteManagerHelper.GetTypeFromDef(siteData._file.Type.DefName);
                if (!string.IsNullOrEmpty(client.UserFile.GuildName)) siteFile.GuildName = client.UserFile.GuildName;
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._file.Tile);
            if (siteFile.Username == client.UserFile.Username) DestroySiteFromFile(siteFile);
            else ResponseShortcutManager.SendNoPowerPacket(client);
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._file = siteFile;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.SiteManager, siteData);

            File.Delete(Path.Combine(Master.SitesPath, siteFile.Tile + CommonValues.DefaultSaveFormat));

            InformationDisplayer.DisplayRemoveSite(siteFile.Tile.ToString());
        }

        private static void ManageWorker(ServerClient client, SiteData data)
        {
            SiteFile site = SiteManagerHelper.GetSiteFileFromTile(data._file.Tile);
            site.WorkerString = data._file.WorkerString;
            site.SaveSite();
        }

        public static void SendRewardsToEveryPlayer()
        {
            foreach (ServerClient client in ServerNetwork.Instance.GetConnectedClientsSafe())
            {
                SendRewardsToPlayer(client);
            }
        }

        public static void SendRewardsToPlayer(ServerClient client)
        {
            SiteFile[] availableSites = SiteManagerHelper.GetAllSites().Where(fetch => (fetch.Username == client.UserFile.Username ||
                (client.UserFile.GuildName != null && client.UserFile.GuildName == fetch.GuildName)) && !string.IsNullOrEmpty(fetch.WorkerString)).ToArray();

            if (availableSites.Length > 0)
            {
                List<SiteReward> toReward = new List<SiteReward>();
                foreach (SiteFile site in availableSites) toReward.Add(client.UserFile.SiteConfigs.First(fetch => fetch.DefName == site.Type.DefName).Reward);

                SiteData siteData = new SiteData();
                siteData._stepMode = SiteStepMode.Rewards;
                siteData._rewardFiles = toReward.ToArray();
                client.Listener.EnqueuePacket(PacketHeader.SiteManager, siteData);
            }
        }

        public static void ChangeUserSiteConfig(ServerClient client, SiteData data)
        {
            SiteRewardConfigData config = data._rewardConfig;

            PlayerSiteConfig toFind = client.UserFile.SiteConfigs.First(fetch => fetch.DefName == config._siteDef);
            toFind.Reward.DefName = config._rewardDef;

            SiteType type = Master.ActionConfigs.SiteAction.SiteTypes.First(fetch => fetch.DefName == config._siteDef);
            toFind.Reward.Amount = type.Rewards.First(fetch => fetch.DefName == config._rewardDef).Amount;

            client.UserFile.SaveUserFile();
        }

        public static void SetSiteInfoForClient(ServerClient client)
        {
            if (client.UserFile.SiteConfigs.Length > 0) return;
            else client.UserFile.UpdateSiteConfigs(Master.ActionConfigs.SiteAction.SiteTypes);
        }
    }

    public static class SiteManagerHelper
    {
        public static SiteFile[] GetAllSitesFromUsername(string username)
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Username == username) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData data)
        {
            SiteFile siteFile = GetSiteFileFromTile(data._file.Tile);
            data._stepMode = SiteStepMode.Info;
            data._file = siteFile;

            client.Listener.EnqueuePacket(PacketHeader.SiteManager, data);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();
            try
            {
                string[] sites = Directory.GetFiles(Master.SitesPath);
                foreach (string site in sites) sitesList.Add(Serializer.SerializeFromFile<SiteFile>(site));
            }
            catch (Exception ex) { Printer.Error($"Sites could not be loaded, either your formatting is wrong in the file 'SiteConfig.json' or you have not updated your sites to the newest version ('Update' command).\n\n{ex.ToString()}"); }

            return sitesList.ToArray();
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static SiteType GetTypeFromDef(string defName)
        {
            SiteType site = Master.ActionConfigs.SiteAction.SiteTypes.Where(S => S.DefName == defName).FirstOrDefault();
            if (site != null) return site;
            return null;
        }
    }
}
