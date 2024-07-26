using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class GoodwillManager
    {
        public static void ChangeUserGoodwills(ServerClient client, Packet packet)
        {
            FactionGoodwillData factionGoodwillData = Serializer.ConvertBytesToObject<FactionGoodwillData>(packet.contents);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionGoodwillData.tile);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(factionGoodwillData.tile);

            if (settlementFile != null) factionGoodwillData.owner = settlementFile.owner;
            else factionGoodwillData.owner = siteFile.owner;

            if (client.userFile.HasFaction && FactionManager.GetFactionFromClient(client).factionMembers.Contains(factionGoodwillData.owner))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.userFile.EnemyPlayers.Remove(factionGoodwillData.owner);
            client.userFile.AllyPlayers.Remove(factionGoodwillData.owner);

            if (factionGoodwillData.goodwill == Goodwill.Enemy)
            {
                if (!client.userFile.EnemyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.userFile.EnemyPlayers.Add(factionGoodwillData.owner);
                }
            }

            else if (factionGoodwillData.goodwill == Goodwill.Ally)
            {
                if (!client.userFile.AllyPlayers.Contains(factionGoodwillData.owner))
                {
                    client.userFile.AllyPlayers.Add(factionGoodwillData.owner);
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

            client.userFile.SaveUserFile();

            Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(rPacket);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.owner;
            else usernameToCheck = siteFile.owner;

            if (client.userFile.HasFaction && FactionManager.GetFactionFromFactionName(client.userFile.FactionName).factionMembers.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.userFile.Username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.EnemyPlayers.Contains(usernameToCheck)) return Goodwill.Enemy;
            else if (client.userFile.AllyPlayers.Contains(usernameToCheck)) return Goodwill.Ally;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSettlementGoodwill(ServerClient client, SettlementFile settlement)
        {
            if (client.userFile.HasFaction && FactionManager.GetFactionFromFactionName(client.userFile.FactionName).factionMembers.Contains(settlement.owner))
            {
                if (settlement.owner == client.userFile.Username) return Goodwill.Personal;
                else return Goodwill.Faction;
            }

            else if (client.userFile.EnemyPlayers.Contains(settlement.owner)) return Goodwill.Enemy;
            else if (client.userFile.AllyPlayers.Contains(settlement.owner)) return Goodwill.Ally;
            else if (settlement.owner == client.userFile.Username) return Goodwill.Personal;
            else return Goodwill.Neutral;
        }

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteFile site)
        {
            if (site.isFromFaction)
            {
                if (client.userFile.HasFaction && client.userFile.FactionName == site.factionName) return Goodwill.Faction;

                else if (client.userFile.EnemyPlayers.Contains(site.owner)) return Goodwill.Enemy;

                else if (client.userFile.AllyPlayers.Contains(site.owner)) return Goodwill.Ally;

                FactionFile factionFile = FactionManager.GetFactionFromFactionName(site.factionName);

                foreach(string str in client.userFile.EnemyPlayers)
                {
                    if (FactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.userFile.AllyPlayers)
                {
                    if (FactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }

                return Goodwill.Neutral;
            }

            else
            {
                if (site.owner == client.userFile.Username) return Goodwill.Personal;
                else if (client.userFile.EnemyPlayers.Contains(site.owner)) return Goodwill.Enemy;
                else if (client.userFile.AllyPlayers.Contains(site.owner)) return Goodwill.Ally;
                else return Goodwill.Neutral;
            }
        }

        public static void ClearAllFactionMemberGoodwills(FactionFile factionFile)
        {
            ServerClient[] clients = Network.connectedClients.ToArray();
            List<ServerClient> clientsToGet = new List<ServerClient>();

            foreach (ServerClient client in clients)
            {
                if (factionFile.factionMembers.Contains(client.userFile.Username)) clientsToGet.Add(client);
            }

            foreach (ServerClient client in clientsToGet)
            {
                for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                {
                    if (client.userFile.EnemyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        client.userFile.EnemyPlayers.Remove(factionFile.factionMembers[i]);
                    }

                    else if (client.userFile.AllyPlayers.Contains(factionFile.factionMembers[i]))
                    {
                        client.userFile.AllyPlayers.Remove(factionFile.factionMembers[i]);
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
                if (settlement.owner == client.userFile.Username) continue;

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

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.GoodwillPacket), factionGoodwillData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
