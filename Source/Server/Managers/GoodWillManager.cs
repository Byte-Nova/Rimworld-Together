using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class GoodwillManager
    {
        public static void ChangeUserGoodwills(ServerClient client, Packet packet)
        {
            FactionGoodwillData factionGoodwillData = Serializer.ConvertBytesToObject<FactionGoodwillData>(packet.contents);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionGoodwillData._tile);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(factionGoodwillData._tile);

            if (settlementFile != null) factionGoodwillData._owner = settlementFile.Owner;
            else factionGoodwillData._owner = siteFile.Owner;

            if (client.userFile.FactionFile != null && client.userFile.FactionFile.CurrentMembers.Contains(factionGoodwillData._owner))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.userFile.Relationships.EnemyPlayers.Remove(factionGoodwillData._owner);
            client.userFile.Relationships.AllyPlayers.Remove(factionGoodwillData._owner);

            if (factionGoodwillData._goodwill == Goodwill.Enemy)
            {
                if (!client.userFile.Relationships.EnemyPlayers.Contains(factionGoodwillData._owner))
                {
                    client.userFile.Relationships.EnemyPlayers.Add(factionGoodwillData._owner);
                }
            }

            else if (factionGoodwillData._goodwill == Goodwill.Ally)
            {
                if (!client.userFile.Relationships.AllyPlayers.Contains(factionGoodwillData._owner))
                {
                    client.userFile.Relationships.AllyPlayers.Add(factionGoodwillData._owner);
                }
            }

            List<Goodwill> tempSettlementList = new List<Goodwill>();
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                //Check if settlement owner is the one we are looking for

                if (settlement.Owner == factionGoodwillData._owner)
                {
                    factionGoodwillData._settlementTiles.Add(settlement.Tile);
                    tempSettlementList.Add(GetSettlementGoodwill(client, settlement));
                }
            }
            factionGoodwillData._settlementGoodwills = tempSettlementList.ToArray();

            List<Goodwill> tempSiteList = new List<Goodwill>();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteFile site in sites)
            {
                //Check if site owner is the one we are looking for

                if (site.Owner == factionGoodwillData._owner)
                {
                    factionGoodwillData._siteTiles.Add(site.Tile);
                    tempSiteList.Add(GetSiteGoodwill(client, site));
                }
            }
            factionGoodwillData._siteGoodwills = tempSiteList.ToArray();

            UserManagerHelper.SaveUserFile(client.userFile);

            Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(rPacket);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.Owner;
            else usernameToCheck = siteFile.Owner;

            if (client.userFile.FactionFile != null && FactionManagerHelper.GetFactionFromFactionName(client.userFile.FactionFile.Name).CurrentMembers.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.userFile.Username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.Relationships.EnemyPlayers.Contains(usernameToCheck)) return Goodwill.Enemy;
            else if (client.userFile.Relationships.AllyPlayers.Contains(usernameToCheck)) return Goodwill.Ally;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (client.userFile.FactionFile != null && FactionManagerHelper.GetFactionFromFactionName(client.userFile.FactionFile.Name).CurrentMembers.Contains(settlement.Owner))
            {
                if (settlement.Owner == client.userFile.Username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.Relationships.EnemyPlayers.Contains(settlement.Owner)) return Goodwill.Enemy;
            else if (client.userFile.Relationships.AllyPlayers.Contains(settlement.Owner)) return Goodwill.Ally;
            else if (settlement.Owner == client.userFile.Username) return Goodwill.Personal;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (site.FactionFile != null)
            {
                if (client.userFile.FactionFile != null && client.userFile.FactionFile.Name == site.FactionFile.Name) return Goodwill.Faction;

                else if (client.userFile.Relationships.EnemyPlayers.Contains(site.Owner)) return Goodwill.Enemy;

                else if (client.userFile.Relationships.AllyPlayers.Contains(site.Owner)) return Goodwill.Ally;

                FactionFile factionFile = FactionManagerHelper.GetFactionFromFactionName(site.FactionFile.Name);

                foreach(string str in client.userFile.Relationships.EnemyPlayers)
                {
                    if (FactionManagerHelper.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.userFile.Relationships.AllyPlayers)
                {
                    if (FactionManagerHelper.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }

                return Goodwill.Neutral;
            }

            else
            {
                if (site.Owner == client.userFile.Username) return Goodwill.Personal;
                else if (client.userFile.Relationships.EnemyPlayers.Contains(site.Owner)) return Goodwill.Enemy;
                else if (client.userFile.Relationships.AllyPlayers.Contains(site.Owner)) return Goodwill.Ally;
                else return Goodwill.Neutral;
            }
        }

        public static void ClearAllFactionMemberGoodwills(FactionFile factionFile)
        {
            ServerClient[] clients = NetworkHelper.GetConnectedClientsSafe();
            List<ServerClient> clientsToGet = new List<ServerClient>();

            foreach (ServerClient client in clients)
            {
                if (factionFile.CurrentMembers.Contains(client.userFile.Username)) clientsToGet.Add(client);
            }

            foreach (ServerClient client in clientsToGet)
            {
                for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
                {
                    if (client.userFile.Relationships.EnemyPlayers.Contains(factionFile.CurrentMembers[i]))
                    {
                        client.userFile.Relationships.EnemyPlayers.Remove(factionFile.CurrentMembers[i]);
                    }

                    else if (client.userFile.Relationships.AllyPlayers.Contains(factionFile.CurrentMembers[i]))
                    {
                        client.userFile.Relationships.AllyPlayers.Remove(factionFile.CurrentMembers[i]);
                    }
                }
            }

            UserFile[] userFiles = UserManagerHelper.GetAllUserFiles();
            List<UserFile> usersToGet = new List<UserFile>();

            foreach (UserFile file in userFiles)
            {
                if (factionFile.CurrentMembers.Contains(file.Username)) usersToGet.Add(file);
            }

            foreach (UserFile file in usersToGet)
            {
                for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
                {
                    if (file.Relationships.EnemyPlayers.Contains(factionFile.CurrentMembers[i]))
                    {
                        file.Relationships.EnemyPlayers.Remove(factionFile.CurrentMembers[i]);
                    }

                    else if (file.Relationships.AllyPlayers.Contains(factionFile.CurrentMembers[i]))
                    {
                        file.Relationships.AllyPlayers.Remove(factionFile.CurrentMembers[i]);
                    }
                }

                UserManagerHelper.SaveUserFile(file);
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
                if (settlement.Owner == client.userFile.Username) continue;

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

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
