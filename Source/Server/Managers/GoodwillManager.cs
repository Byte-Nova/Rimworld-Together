using Shared;
using System.Linq;
using static Shared.CommonEnumerators;

namespace GameServer
{
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
            SiteIdendityFile siteFile = SiteManagerHelper.GetSiteFileFromTile(data._tile);

            if (settlementFile != null) data._owner = settlementFile.Owner;
            else data._owner = siteFile.Owner;

            if (client.userFile.FactionFile != null && client.userFile.FactionFile.CurrentMembers.Contains(data._owner))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.userFile.Relationships.EnemyPlayers.Remove(data._owner);
            client.userFile.Relationships.AllyPlayers.Remove(data._owner);

            if (data._goodwill == Goodwill.Enemy)
            {
                if (!client.userFile.Relationships.EnemyPlayers.Contains(data._owner))
                {
                    client.userFile.Relationships.EnemyPlayers.Add(data._owner);
                }
            }

            else if (data._goodwill == Goodwill.Ally)
            {
                if (!client.userFile.Relationships.AllyPlayers.Contains(data._owner))
                {
                    client.userFile.Relationships.AllyPlayers.Add(data._owner);
                }
            }

            List<Goodwill> tempSettlementList = new List<Goodwill>();
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                //Check if settlement owner is the one we are looking for

                if (settlement.Owner == data._owner)
                {
                    data._settlementTiles.Add(settlement.Tile);
                    tempSettlementList.Add(GetSettlementGoodwill(client, settlement));
                }
            }
            data._settlementGoodwills = tempSettlementList.ToArray();

            List<Goodwill> tempSiteList = new List<Goodwill>();
            SiteIdendityFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteIdendityFile site in sites)
            {
                //Check if site owner is the one we are looking for

                if (site.Owner == data._owner)
                {
                    data._siteTiles.Add(site.Tile);
                    tempSiteList.Add(GetSiteGoodwill(client, site));
                }
            }
            data._siteGoodwills = tempSiteList.ToArray();

            UserManagerHelper.SaveUserFile(client.userFile);

            Packet rPacket = Packet.CreatePacketFromObject(nameof(GoodwillManager), data);
            client.listener.EnqueuePacket(rPacket);
        }

        public static Goodwill GetGoodwillFromTile(ServerClient client, int tileToCheck)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteIdendityFile siteFile = SiteManagerHelper.GetSiteFileFromTile(tileToCheck);

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

        public static Goodwill GetSiteGoodwill(ServerClient client, SiteIdendityFile site)
        {
            if (client.userFile.Username == site.Owner) return Goodwill.Personal; //We check if the players is the owner

            if (site.FactionFile != null)
            {
                FactionFile factionFile = FactionManagerHelper.GetFactionFromFactionName(site.FactionFile.Name);

                if (client.userFile.FactionFile != null && client.userFile.FactionFile.Name == factionFile.Name) return Goodwill.Faction; // We check if the player is in the faction

                foreach (string str in client.userFile.Relationships.EnemyPlayers) // We check if the player is enemy with the faction
                {
                    if (FactionManagerHelper.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Enemy;
                    }
                }

                foreach (string str in client.userFile.Relationships.AllyPlayers) // We check if the player is allied with the faction
                {
                    if (FactionManagerHelper.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return Goodwill.Ally;
                    }
                }
            }
            else
            {
                if (client.userFile.Relationships.EnemyPlayers.Contains(site.Owner)) return Goodwill.Enemy; //We check if the player is enemy of the owner

                else if (client.userFile.Relationships.AllyPlayers.Contains(site.Owner)) return Goodwill.Ally; // We check if the player is allied to the owner
            }
            return Goodwill.Neutral;
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
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements();

            FactionGoodwillData factionGoodwillData = new FactionGoodwillData();
            SiteIdendityFile[] sites = SiteManagerHelper.GetAllSites();

            List<Goodwill> tempList = new List<Goodwill>();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.Owner == client.userFile.Username) continue;

                factionGoodwillData._settlementTiles.Add(settlement.Tile);
                tempList.Add(GetSettlementGoodwill(client, settlement));
            }
            factionGoodwillData._settlementGoodwills = tempList.ToArray();

            tempList = new List<Goodwill>();
            foreach (SiteIdendityFile site in sites)
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