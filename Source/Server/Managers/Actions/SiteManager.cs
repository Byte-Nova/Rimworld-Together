using GameServer.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class SiteManager
    {
        public enum SiteStepMode { Accept, Build, Destroy, Info, Deposit, Retrieve, Reward }

        private enum PersonalSiteType { Farmland, Quarry, Sawmill, Storage }

        private enum FactionSiteType { Bank, Silo }

        public static void ParseSitePacket(Client client, Packet packet)
        {
            SiteDetailsJSON siteDetailsJSON = Serializer.SerializeFromString<SiteDetailsJSON>(packet.contents[0]);

            switch(int.Parse(siteDetailsJSON.siteStep))
            {
                case (int)SiteStepMode.Build:
                    AddNewSite(client, siteDetailsJSON);
                    break;

                case (int)SiteStepMode.Destroy:
                    DestroySite(client, siteDetailsJSON);
                    break;

                case (int)SiteStepMode.Info:
                    GetSiteInfo(client, siteDetailsJSON);
                    break;

                case (int)SiteStepMode.Deposit:
                    DepositWorkerToSite(client, siteDetailsJSON);
                    break;

                case (int)SiteStepMode.Retrieve:
                    RetrieveWorkerFromSite(client, siteDetailsJSON);
                    break;
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] sites = Directory.GetFiles(Program.sitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToCheck) return true;
            }

            return false;
        }

        public static void ConfirmNewSite(Client client, SiteFile siteFile)
        {
            SaveSite(siteFile);

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)SiteStepMode.Build).ToString();
            siteDetailsJSON.tile = siteFile.tile;
            siteDetailsJSON.owner = client.username;
            siteDetailsJSON.type = siteFile.type;
            siteDetailsJSON.isFromFaction = siteFile.isFromFaction;

            foreach (Client cClient in Network.connectedClients.ToArray())
            {
                siteDetailsJSON.likelihood = LikelihoodManager.GetSiteLikelihood(cClient, siteFile).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
                Packet packet = new Packet("SitePacket", contents);

                Network.SendData(cClient, packet);
            }

            siteDetailsJSON.siteStep = ((int)SiteStepMode.Accept).ToString();
            string[] contents2 = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
            Packet rPacket = new Packet("SitePacket", contents2);
            Network.SendData(client, rPacket);

            Logger.WriteToConsole($"[Created site] > {client.username}", Logger.LogMode.Warning);
        }

        public static void SaveSite(SiteFile siteFile)
        {
            Serializer.SerializeToFile(Path.Combine(Program.sitesPath, siteFile.tile + ".json"), siteFile);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Program.sitesPath);
            foreach (string site in sites)
            {
                sitesList.Add(Serializer.SerializeFromFile<SiteFile>(site));
            }

            return sitesList.ToArray();
        }

        public static SiteFile[] GetAllSitesFromUser(Client client)
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Program.sitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (!siteFile.isFromFaction && siteFile.owner == client.username)
                {
                    sitesList.Add(siteFile);
                }
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(string tileToGet)
        {
            string[] sites = Directory.GetFiles(Program.sitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToGet) return siteFile;
            }

            return null;
        }

        private static void AddNewSite(Client client, SiteDetailsJSON siteDetailsJSON)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client);
            else if (CheckIfTileIsInUse(siteDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                SiteFile siteFile = null;

                if (siteDetailsJSON.isFromFaction)
                {
                    FactionFile factionFile = FactionManager.GetFactionFromClient(client);

                    if (FactionManager.GetMemberRank(factionFile, client.username) == FactionManager.FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new FactionManifestJSON());
                        return;
                    }

                    else
                    {
                        siteFile = new SiteFile();
                        siteFile.tile = siteDetailsJSON.tile;
                        siteFile.owner = client.username;
                        siteFile.type = siteDetailsJSON.type;
                        siteFile.isFromFaction = true;
                        siteFile.factionName = client.factionName;
                    }
                }

                else
                {
                    siteFile = new SiteFile();
                    siteFile.tile = siteDetailsJSON.tile;
                    siteFile.owner = client.username;
                    siteFile.type = siteDetailsJSON.type;
                    siteFile.isFromFaction = false;
                }

                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(Client client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.isFromFaction)
            {
                if (siteFile.factionName != client.factionName) ResponseShortcutManager.SendIllegalPacket(client);
                else
                {
                    FactionFile factionFile = FactionManager.GetFactionFromClient(client);

                    if (FactionManager.GetMemberRank(factionFile, client.username) != 
                        FactionManager.FactionRanks.Member) DestroySiteFromFile(siteFile);

                    else ResponseShortcutManager.SendNoPowerPacket(client, new FactionManifestJSON());
                }
            }

            else
            {
                if (siteFile.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client);
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)SiteStepMode.Destroy).ToString();
            siteDetailsJSON.tile = siteFile.tile;

            string[] contents = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
            Packet packet = new Packet("SitePacket", contents);
            foreach (Client client in Network.connectedClients.ToArray()) Network.SendData(client, packet);

            File.Delete(Path.Combine(Program.sitesPath, siteFile.tile + ".json"));
            Logger.WriteToConsole($"[Destroyed site] > {siteFile.tile}", Logger.LogMode.Warning);
        }

        private static void GetSiteInfo(Client client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            siteDetailsJSON.type = siteFile.type;
            siteDetailsJSON.workerData = siteFile.workerData;
            siteDetailsJSON.isFromFaction = siteFile.isFromFaction;

            string[] contents = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
            Packet packet = new Packet("SitePacket", contents);
            Network.SendData(client, packet);
        }

        private static void DepositWorkerToSite(Client client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.owner != client.username &&
                FactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client);
            }

            else
            {
                siteFile.workerData = siteDetailsJSON.workerData;
                SaveSite(siteFile);
            }
        }

        private static void RetrieveWorkerFromSite(Client client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.owner != client.username &&
                FactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client);
            }

            else
            {
                siteDetailsJSON.workerData = siteFile.workerData;
                siteFile.workerData = "";

                SaveSite(siteFile);

                string[] contents = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
                Packet packet = new Packet("SitePacket", contents);
                Network.SendData(client, packet);
            }
        }

        public static void StartSiteTicker()
        {
            while (true)
            {
                Thread.Sleep(1800000);

                SiteRewardTick();
            }
        }

        private static void SiteRewardTick()
        {
            SiteFile[] sites = GetAllSites();

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)SiteStepMode.Reward).ToString();

            foreach (Client client in Network.connectedClients.ToArray())
            {
                siteDetailsJSON.sitesWithRewards.Clear();

                List<SiteFile> playerSites = sites.ToList().FindAll(x => x.owner == client.username);
                foreach (SiteFile site in playerSites)
                {
                    if (!string.IsNullOrWhiteSpace(site.workerData) && !site.isFromFaction)
                    {
                        siteDetailsJSON.sitesWithRewards.Add(site.tile);
                        continue;
                    }
                }

                if (client.hasFaction)
                {
                    List<SiteFile> factionSites = sites.ToList().FindAll(x => x.factionName == client.factionName);
                    foreach (SiteFile site in factionSites)
                    {
                        if (site.isFromFaction) siteDetailsJSON.sitesWithRewards.Add(site.tile);
                    }
                }

                if (siteDetailsJSON.sitesWithRewards.Count() > 0)
                {
                    string[] contents = new string[] { Serializer.SerializeToString(siteDetailsJSON) };
                    Packet packet = new Packet("SitePacket", contents);
                    Network.SendData(client, packet);
                }

                Console.WriteLine($"Player {client.username} Count > {siteDetailsJSON.sitesWithRewards.Count()}");
            }

            Logger.WriteToConsole($"[Site tick]");
        }
    }
}
