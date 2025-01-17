using GameServer.Files;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class GoodwillManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            FactionGoodwillData data = Serializer.ConvertBytesToObject<FactionGoodwillData>(packet.contents);
            ChangeUserGoodwills(client, data);

        }

        public static void ChangeUserGoodwills(ServerClient client, FactionGoodwillData data)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._tile);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(data._tile);

            if (settlementFile != null) data._uid = settlementFile.UID;
            else data._uid = siteFile.UID;

            if (GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName).CurrentUids.Contains(data._uid))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.userFile.EnemyPlayers.Remove(data._uid);
            client.userFile.AllyPlayers.Remove(data._uid);

            if (data._goodwill == Goodwill.Enemy)
            {
                if (!client.userFile.EnemyPlayers.Contains(data._uid))
                {
                    client.userFile.EnemyPlayers.Add(data._uid);
                }
            }

            else if (data._goodwill == Goodwill.Ally)
            {
                if (!client.userFile.AllyPlayers.Contains(data._uid))
                {
                    client.userFile.AllyPlayers.Add(data._uid);
                }
            }

            List<Goodwill> tempSettlementList = new List<Goodwill>();
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements();
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

            UserManagerH.SaveUserFile(client.userFile);

            Packet rPacket = Packet.CreatePacketFromObject(nameof(GoodwillManager), data);
            client.listener.EnqueuePacket(rPacket);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.UID;
            else usernameToCheck = siteFile.UID;

            if (GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName).CurrentUids.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.userFile.Uid) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.EnemyPlayers.Contains(usernameToCheck)) return Goodwill.Enemy;
            else if (client.userFile.AllyPlayers.Contains(usernameToCheck)) return Goodwill.Ally;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName).CurrentUids.Contains(settlement.UID))
            {
                if (settlement.UID == client.userFile.Uid) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.EnemyPlayers.Contains(settlement.UID)) return Goodwill.Enemy;
            else if (client.userFile.AllyPlayers.Contains(settlement.UID)) return Goodwill.Ally;
            else if (settlement.UID == client.userFile.Uid) return Goodwill.Personal;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (client.userFile.Uid == site.UID) return Goodwill.Personal; //We check if the players is the owner

            if (!string.IsNullOrEmpty(site.GuildName))
            {
                GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(site.GuildName);

                if (client.userFile.GuildName == factionFile.Name) return Goodwill.Faction; // We check if the player is in the faction

                foreach (string str in client.userFile.EnemyPlayers) // We check if the player is enemy with the faction
                {
                    if (GuildManagerH.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.userFile.AllyPlayers) // We check if the player is allied with the faction
                {
                    if (GuildManagerH.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }
            }
            else
            {
                if (client.userFile.EnemyPlayers.Contains(site.UID)) return Goodwill.Enemy; //We check if the player is enemy of the owner

                else if (client.userFile.AllyPlayers.Contains(site.UID)) return Goodwill.Ally; // We check if the player is allied to the owner
            }
            return Goodwill.Neutral;
        }

        public static void ClearAllFactionMemberGoodwills(GuildFile factionFile)
        {
            ServerClient[] clients = NetworkHelper.GetConnectedClientsSafe();
            List<ServerClient> clientsToGet = new List<ServerClient>();

            foreach (ServerClient client in clients)
            {
                if (factionFile.CurrentUids.Contains(client.userFile.Uid)) clientsToGet.Add(client);
            }

            foreach (ServerClient client in clientsToGet)
            {
                for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                {
                    if (client.userFile.EnemyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        client.userFile.EnemyPlayers.Remove(factionFile.CurrentUids[i]);
                    }

                    else if (client.userFile.AllyPlayers.Contains(factionFile.CurrentUids[i]))
                    {
                        client.userFile.AllyPlayers.Remove(factionFile.CurrentUids[i]);
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
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements();

            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();

            List<Goodwill> tempList = new List<Goodwill>();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.UID == client.userFile.Uid) continue;

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

            Packet packet = Packet.CreatePacketFromObject(nameof(GoodwillManager), factionGoodwillData);
            client.listener.EnqueuePacket(packet);
        }
    }
}