using Shared;

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

            if (factionGoodwillData.goodwill == ((int)CommonEnumerators.Goodwills.Enemy).ToString())
            {
                if (!client.enemyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.enemyPlayers.Add(factionGoodwillData.owner);
                }
            }

            else if (factionGoodwillData.goodwill == ((int)CommonEnumerators.Goodwills.Ally).ToString())
            {
                if (!client.allyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.allyPlayers.Add(factionGoodwillData.owner);
                }
            }

            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == factionGoodwillData.owner)
                {
                    factionGoodwillData.settlementTiles.Add(settlement.tile);
                    factionGoodwillData.settlementGoodwills.Add(GetSettlementGoodwill(client, settlement).ToString());
                }
            }

            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                if (site.isFromFaction)
                {
                    if (site.factionName == UserManager.GetUserFileFromName(factionGoodwillData.owner).factionName)
                    {
                        factionGoodwillData.siteTiles.Add(site.tile);
                        factionGoodwillData.siteGoodwills.Add(GetSiteGoodwill(client, site).ToString());
                    }
                }

                else
                {
                    if (site.owner == factionGoodwillData.owner)
                    {
                        factionGoodwillData.siteTiles.Add(site.tile);
                        factionGoodwillData.siteGoodwills.Add(GetSiteGoodwill(client, site).ToString());
                    }
                }
            }

            UserFile userFile = UserManager.GetUserFile(client);
            userFile.enemyPlayers = client.enemyPlayers;
            userFile.allyPlayers = client.allyPlayers;
            UserManager.SaveUserFile(client, userFile);

            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(rPacket);
        }

        public static int GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.owner;
            else usernameToCheck = siteFile.owner;

            if (client.hasFaction && OnlineFactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.username) return (int)CommonEnumerators.Goodwills.Personal;
                else return (int)CommonEnumerators.Goodwills.Faction;
            }

            else if (client.enemyPlayers.Contains(usernameToCheck)) return (int)CommonEnumerators.Goodwills.Enemy;
            else if (client.allyPlayers.Contains(usernameToCheck)) return (int)CommonEnumerators.Goodwills.Ally;
            else return (int)CommonEnumerators.Goodwills.Neutral;
        }

        public static int GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (client.hasFaction && OnlineFactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(settlement.owner))
            {
                if (settlement.owner == client.username) return (int)CommonEnumerators.Goodwills.Personal;
                else return (int)CommonEnumerators.Goodwills.Faction;
            }

            else if (client.enemyPlayers.Contains(settlement.owner)) return (int)CommonEnumerators.Goodwills.Enemy;
            else if (client.allyPlayers.Contains(settlement.owner)) return (int)CommonEnumerators.Goodwills.Ally;
            else if (settlement.owner == client.username) return (int)CommonEnumerators.Goodwills.Personal;
            else return (int)CommonEnumerators.Goodwills.Neutral;
        }

        public static int GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (site.isFromFaction)
            {
                if (client.hasFaction && client.factionName == site.factionName) return (int)CommonEnumerators.Goodwills.Faction;

                else if (client.enemyPlayers.Contains(site.owner)) return (int)CommonEnumerators.Goodwills.Enemy;

                else if (client.allyPlayers.Contains(site.owner)) return (int)CommonEnumerators.Goodwills.Ally;

                FactionFile factionFile = OnlineFactionManager.GetFactionFromFactionName(site.factionName);

                foreach(string str in client.enemyPlayers)
                {
                    if (OnlineFactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return (int)CommonEnumerators.Goodwills.Enemy;
                    }
                }

                foreach (string str in client.allyPlayers)
                {
                    if (OnlineFactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return (int)CommonEnumerators.Goodwills.Ally;
                    }
                }

                return (int)CommonEnumerators.Goodwills.Neutral;
            }

            else
            {
                if (site.owner == client.username) return (int)CommonEnumerators.Goodwills.Personal;
                else if (client.enemyPlayers.Contains(site.owner)) return (int)CommonEnumerators.Goodwills.Enemy;
                else if (client.allyPlayers.Contains(site.owner)) return (int)CommonEnumerators.Goodwills.Ally;
                else return (int)CommonEnumerators.Goodwills.Neutral;
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
                if (factionFile.factionMembers.Contains(file.username)) usersToGet.Add(file);
            }

            foreach (UserFile file in usersToGet)
            {
                for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                {
                    if (file.enemyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        file.enemyPlayers.Remove(factionFile.factionMembers[i]);
                    }

                    else if (file.allyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        file.allyPlayers.Remove(factionFile.factionMembers[i]);
                    }
                }

                UserManager.SaveUserFileFromName(file.username, file);
            }
        }

        public static void UpdateClientGoodwills(ServerClient client)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            SiteFile[] sites = SiteManager.GetAllSites();

            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();

            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == client.username) continue;

                factionGoodwillData.settlementTiles.Add(settlement.tile);
                factionGoodwillData.settlementGoodwills.Add(GetSettlementGoodwill(client, settlement).ToString());
            }

            foreach (SiteFile site in sites)
            {
                factionGoodwillData.siteTiles.Add(site.tile);
                factionGoodwillData.siteGoodwills.Add(GetSiteGoodwill(client, site).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
