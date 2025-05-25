using GameServer.Files;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class GoodwillManager
    {
        [HandlesPacket(PacketHeader.GoodWillManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            FactionGoodwillData data = Serializer.ConvertBytesToObject<FactionGoodwillData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            ChangeUserGoodwills(client, data);
        }

        public static void ChangeUserGoodwills(ServerClient client, FactionGoodwillData data)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._tile);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(data._tile);

            if (settlementFile != null) data._uid = settlementFile.UID;
            else data._uid = siteFile.UID;

            if (GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName).CurrentUids.Contains(data._uid))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.UserFile.EnemyPlayers.Remove(data._uid);
            client.UserFile.AllyPlayers.Remove(data._uid);

            if (data._goodwill == Goodwill.Enemy)
            {
                if (!client.UserFile.EnemyPlayers.Contains(data._uid))
                {
                    client.UserFile.EnemyPlayers.Add(data._uid);
                }
            }

            else if (data._goodwill == Goodwill.Ally)
            {
                if (!client.UserFile.AllyPlayers.Contains(data._uid))
                {
                    client.UserFile.AllyPlayers.Add(data._uid);
                }
            }

            List<Goodwill> tempSettlementList = new List<Goodwill>();
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                //Check if settlement owner is the one we are looking for

                if (settlement.UID == data._uid)
                {
                    data._settlementTiles.Add(settlement.Tile);
                    tempSettlementList.Add(GetSettlementGoodwill(client, settlement));
                }
            }
            data._settlementGoodwills = tempSettlementList.ToArray();

            List<Goodwill> tempSiteList = new List<Goodwill>();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteFile site in sites)
            {
                //Check if site owner is the one we are looking for

                if (site.UID == data._uid)
                {
                    data._siteTiles.Add(site.Tile);
                    tempSiteList.Add(GetSiteGoodwill(client, site));
                }
            }
            data._siteGoodwills = tempSiteList.ToArray();

            UserManagerH.SaveUserFile(client.UserFile);

            client.Listener.EnqueuePacket(PacketHeader.GoodWillManager, data);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.UID;
            else usernameToCheck = siteFile.UID;

            if (GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName).CurrentUids.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.UserFile.Uid) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.UserFile.EnemyPlayers.Contains(usernameToCheck)) return Goodwill.Enemy;
            else if (client.UserFile.AllyPlayers.Contains(usernameToCheck)) return Goodwill.Ally;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName).CurrentUids.Contains(settlement.UID))
            {
                if (settlement.UID == client.UserFile.Uid) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.UserFile.EnemyPlayers.Contains(settlement.UID)) return Goodwill.Enemy;
            else if (client.UserFile.AllyPlayers.Contains(settlement.UID)) return Goodwill.Ally;
            else if (settlement.UID == client.UserFile.Uid) return Goodwill.Personal;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (client.UserFile.Uid == site.UID) return Goodwill.Personal; //We check if the players is the owner

            if (!string.IsNullOrEmpty(site.GuildName))
            {
                GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(site.GuildName);

                if (client.UserFile.GuildName == factionFile.Name) return Goodwill.Faction; // We check if the player is in the faction

                foreach (string str in client.UserFile.EnemyPlayers) // We check if the player is enemy with the faction
                {
                    if (GuildManagerH.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.UserFile.AllyPlayers) // We check if the player is allied with the faction
                {
                    if (GuildManagerH.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }
            }
            else
            {
                if (client.UserFile.EnemyPlayers.Contains(site.UID)) return Goodwill.Enemy; //We check if the player is enemy of the owner

                else if (client.UserFile.AllyPlayers.Contains(site.UID)) return Goodwill.Ally; // We check if the player is allied to the owner
            }
            return Goodwill.Neutral;
        }

        public static void ClearAllFactionMemberGoodwills(GuildFile factionFile)
        {
            ServerClient[] clients = NetworkHelper.GetConnectedClientsSafe();
            List<ServerClient> clientsToGet = new List<ServerClient>();

            foreach (ServerClient client in clients)
            {
                if (factionFile.CurrentUids.Contains(client.UserFile.Uid)) clientsToGet.Add(client);
            }

            foreach (ServerClient client in clientsToGet)
            {
                for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                {
                    if (client.UserFile.EnemyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        client.UserFile.EnemyPlayers.Remove(factionFile.CurrentUids[i]);
                    }

                    else if (client.UserFile.AllyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        client.UserFile.AllyPlayers.Remove(factionFile.CurrentUids[i]);
                    }
                }
            }

            UserFile[] userFiles = UserManagerH.GetAllUserFiles();
            List<UserFile> usersToGet = new List<UserFile>();

            foreach (UserFile file in userFiles)
            {
                if (factionFile.CurrentUids.Contains(file.Uid)) usersToGet.Add(file);
            }

            foreach (UserFile file in usersToGet)
            {
                for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                {
                    if (file.EnemyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        file.EnemyPlayers.Remove(factionFile.CurrentUids[i]);
                    }

                    else if (file.AllyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        file.AllyPlayers.Remove(factionFile.CurrentUids[i]);
                    }
                }

                UserManagerH.SaveUserFile(file);
            }
        }

        public static void UpdateClientGoodwills(ServerClient client)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();

            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();

            List<Goodwill> tempList = new List<Goodwill>();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.UID == client.UserFile.Uid) continue;

                factionGoodwillData._settlementTiles.Add(settlement.Tile);
                tempList.Add(GetSettlementGoodwill(client, settlement));
            }
            factionGoodwillData._settlementGoodwills = tempList.ToArray();

            tempList = new List<Goodwill>();
            foreach (SiteFile site in sites)
            {
                factionGoodwillData._siteTiles.Add(site.Tile);
                tempList.Add(GetSiteGoodwill(client, site));
            }
            factionGoodwillData._siteGoodwills = tempList.ToArray();

            client.Listener.EnqueuePacket(PacketHeader.GoodWillManager, factionGoodwillData);
        }
    }
}