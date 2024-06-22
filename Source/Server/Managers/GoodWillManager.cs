using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class GoodwillManager
    {
        public static void ChangeUserGoodwills(ServerClient client, Packet packet)
        {
            FactionGoodwillData factionGoodwillData = (FactionGoodwillData)Serializer.ConvertBytesToObject(packet.contents);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionGoodwillData.tile);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(factionGoodwillData.tile);

            if (settlementFile != null) factionGoodwillData.owner = settlementFile.owner;
            else factionGoodwillData.owner = siteFile.owner;

            if (client.hasFaction && OnlineFactionManager.GetFactionFromClient(client).factionMembers.Contains(factionGoodwillData.owner))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.enemyPlayers.Remove(factionGoodwillData.owner);
            client.allyPlayers.Remove(factionGoodwillData.owner);

            if (factionGoodwillData.goodwill == Goodwill.Enemy)
            {
                if (!client.enemyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.enemyPlayers.Add(factionGoodwillData.owner);
                }
            }

            else if (factionGoodwillData.goodwill == Goodwill.Ally)
            {
                if (!client.allyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.allyPlayers.Add(factionGoodwillData.owner);
                }
            }

            List<Goodwill> tempList = new List<Goodwill>();
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == factionGoodwillData.owner)
                {
                    factionGoodwillData.settlementTiles.Add(settlement.tile);
                    tempList.Add(GetSettlementGoodwill(client, settlement));
                }
            }
            factionGoodwillData.settlementGoodwills = tempList.ToArray();

            tempList = new List<Goodwill>();
            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                if (site.isFromFaction)
                {
                    if (site.factionName == UserManager.GetUserFileFromName(factionGoodwillData.owner).FactionName)
                    {
                        factionGoodwillData.siteTiles.Add(site.tile);
                        tempList.Add(GetSiteGoodwill(client, site));
                    }
                }

                else
                {
                    if (site.owner == factionGoodwillData.owner)
                    {
                        factionGoodwillData.siteTiles.Add(site.tile);
                        tempList.Add(GetSiteGoodwill(client, site));
                    }
                }
            }
            factionGoodwillData.siteGoodwills = tempList.ToArray();

            client.SaveToUserFile();

            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(rPacket);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.owner;
            else usernameToCheck = siteFile.owner;

            if (client.hasFaction && OnlineFactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.enemyPlayers.Contains(usernameToCheck)) return Goodwill.Enemy;
            else if (client.allyPlayers.Contains(usernameToCheck)) return Goodwill.Ally;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (client.hasFaction && OnlineFactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(settlement.owner))
            {
                if (settlement.owner == client.username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.enemyPlayers.Contains(settlement.owner)) return Goodwill.Enemy;
            else if (client.allyPlayers.Contains(settlement.owner)) return Goodwill.Ally;
            else if (settlement.owner == client.username) return Goodwill.Personal;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (site.isFromFaction)
            {
                if (client.hasFaction && client.factionName == site.factionName) return Goodwill.Faction;

                else if (client.enemyPlayers.Contains(site.owner)) return Goodwill.Enemy;

                else if (client.allyPlayers.Contains(site.owner)) return Goodwill.Ally;

                FactionFile factionFile = OnlineFactionManager.GetFactionFromFactionName(site.factionName);

                foreach(string str in client.enemyPlayers)
                {
                    if (OnlineFactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.allyPlayers)
                {
                    if (OnlineFactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }

                return Goodwill.Neutral;
            }

            else
            {
                if (site.owner == client.username) return Goodwill.Personal;
                else if (client.enemyPlayers.Contains(site.owner)) return Goodwill.Enemy;
                else if (client.allyPlayers.Contains(site.owner)) return Goodwill.Ally;
                else return Goodwill.Neutral;
            }
        }

        public static void ClearAllFactionMemberGoodwills(FactionFile factionFile)
        {
            ServerClient[] clients = Network.connectedClients.ToArray();
            List<ServerClient> clientsToGet = new List<ServerClient>();

            foreach (ServerClient client in clients)
            {
                if (factionFile.factionMembers.Contains(client.username)) clientsToGet.Add(client);
            }

            foreach (ServerClient client in clientsToGet)
            {
                for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                {
                    if (client.enemyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        client.enemyPlayers.Remove(factionFile.factionMembers[i]);
                    }

                    else if (client.allyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        client.allyPlayers.Remove(factionFile.factionMembers[i]);
                    }
                }
            }

            UserFile[] userFiles = UserManager.GetAllUserFiles();
            List<UserFile> usersToGet = new List<UserFile>();

            foreach (UserFile file in userFiles)
            {
                if (factionFile.factionMembers.Contains(file.Username)) usersToGet.Add(file);
            }

            foreach (UserFile file in usersToGet)
            {
                for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                {
                    if (file.EnemyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        file.EnemyPlayers.Remove(factionFile.factionMembers[i]);
                    }

                    else if (file.AllyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        file.AllyPlayers.Remove(factionFile.factionMembers[i]);
                    }
                }

                file.SaveUserFile();
            }
        }

        public static void UpdateClientGoodwills(ServerClient client)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            SiteFile[] sites = SiteManager.GetAllSites();

            List<Goodwill> tempList = new List<Goodwill>();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == client.username) continue;

                factionGoodwillData.settlementTiles.Add(settlement.tile);
                tempList.Add(GetSettlementGoodwill(client, settlement));
            }
            factionGoodwillData.settlementGoodwills = tempList.ToArray();

            tempList = new List<Goodwill>();
            foreach (SiteFile site in sites)
            {
                factionGoodwillData.siteTiles.Add(site.tile);
                tempList.Add(GetSiteGoodwill(client, site));
            }
            factionGoodwillData.siteGoodwills = tempList.ToArray();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
