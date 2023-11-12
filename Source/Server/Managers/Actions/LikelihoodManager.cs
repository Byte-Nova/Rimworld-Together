using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class LikelihoodManager
    {
        private enum Likelihoods { Enemy, Neutral, Ally, Faction, Personal }

        public static void ChangeUserLikelihoods(ServerClient client, Packet packet)
        {
            StructureLikelihoodJSON structureLikelihoodJSON = (StructureLikelihoodJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(structureLikelihoodJSON.tile);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(structureLikelihoodJSON.tile);

            if (settlementFile != null) structureLikelihoodJSON.owner = settlementFile.owner;
            else structureLikelihoodJSON.owner = siteFile.owner;

            if (client.hasFaction && FactionManager.GetFactionFromClient(client).factionMembers.Contains(structureLikelihoodJSON.owner))
            {
                ResponseShortcutManager.SendBreakPacket(client);
                return;
            }

            client.enemyPlayers.Remove(structureLikelihoodJSON.owner);
            client.allyPlayers.Remove(structureLikelihoodJSON.owner);

            if (structureLikelihoodJSON.likelihood == ((int)Likelihoods.Enemy).ToString())
            {
                if (!client.enemyPlayers.Contains(structureLikelihoodJSON.owner))
                {
                    client.enemyPlayers.Add(structureLikelihoodJSON.owner);
                }
            }

            else if (structureLikelihoodJSON.likelihood == ((int)Likelihoods.Ally).ToString())
            {
                if (!client.allyPlayers.Contains(structureLikelihoodJSON.owner))
                {
                    client.allyPlayers.Add(structureLikelihoodJSON.owner);
                }
            }

            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == structureLikelihoodJSON.owner)
                {
                    structureLikelihoodJSON.settlementTiles.Add(settlement.tile);
                    structureLikelihoodJSON.settlementLikelihoods.Add(GetSettlementLikelihood(client, settlement).ToString());
                }
            }

            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                if (site.isFromFaction)
                {
                    if (site.factionName == UserManager.GetUserFileFromName(structureLikelihoodJSON.owner).factionName)
                    {
                        structureLikelihoodJSON.siteTiles.Add(site.tile);
                        structureLikelihoodJSON.siteLikelihoods.Add(GetSiteLikelihood(client, site).ToString());
                    }
                }

                else
                {
                    if (site.owner == structureLikelihoodJSON.owner)
                    {
                        structureLikelihoodJSON.siteTiles.Add(site.tile);
                        structureLikelihoodJSON.siteLikelihoods.Add(GetSiteLikelihood(client, site).ToString());
                    }
                }
            }

            UserFile userFile = UserManager.GetUserFile(client);
            userFile.enemyPlayers = client.enemyPlayers;
            userFile.allyPlayers = client.allyPlayers;
            UserManager.SaveUserFile(client, userFile);

            Packet rPacket = Packet.CreatePacketFromJSON("LikelihoodPacket", structureLikelihoodJSON);
            client.clientListener.SendData(rPacket);
        }

        public static int GetLikelihoodFromTile(ServerClient client, string tileToCheck)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(tileToCheck);
            SiteFile siteFile = SiteManager.GetSiteFileFromTile(tileToCheck);

            string usernameToCheck;
            if (settlementFile != null) usernameToCheck = settlementFile.owner;
            else usernameToCheck = siteFile.owner;

            if (client.hasFaction && FactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(usernameToCheck))
            {
                if (usernameToCheck == client.username) return (int)Likelihoods.Personal;
                else return (int)Likelihoods.Faction;
            }

            else if (client.enemyPlayers.Contains(usernameToCheck)) return (int)Likelihoods.Enemy;
            else if (client.allyPlayers.Contains(usernameToCheck)) return (int)Likelihoods.Ally;
            else return (int)Likelihoods.Neutral;
        }

        public static int GetSettlementLikelihood(ServerClient client, SettlementFile settlement)
        {
            if (client.hasFaction && FactionManager.GetFactionFromFactionName(client.factionName).factionMembers.Contains(settlement.owner))
            {
                if (settlement.owner == client.username) return (int)Likelihoods.Personal;
                else return (int)Likelihoods.Faction;
            }

            else if (client.enemyPlayers.Contains(settlement.owner)) return (int)Likelihoods.Enemy;
            else if (client.allyPlayers.Contains(settlement.owner)) return (int)Likelihoods.Ally;
            else if (settlement.owner == client.username) return (int)Likelihoods.Personal;
            else return (int)Likelihoods.Neutral;
        }

        public static int GetSiteLikelihood(ServerClient client, SiteFile site)
        {
            if (site.isFromFaction)
            {
                if (client.hasFaction && client.factionName == site.factionName) return (int)Likelihoods.Faction;

                else if (client.enemyPlayers.Contains(site.owner)) return (int)Likelihoods.Enemy;

                else if (client.allyPlayers.Contains(site.owner)) return (int)Likelihoods.Ally;

                FactionFile factionFile = FactionManager.GetFactionFromFactionName(site.factionName);

                foreach(string str in client.enemyPlayers)
                {
                    if (FactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return (int)Likelihoods.Enemy;
                    }
                }

                foreach (string str in client.allyPlayers)
                {
                    if (FactionManager.CheckIfUserIsInFaction(factionFile, str))
                    {
                        return (int)Likelihoods.Ally;
                    }
                }

                return (int)Likelihoods.Neutral;
            }

            else
            {
                if (site.owner == client.username) return (int)Likelihoods.Personal;
                else if (client.enemyPlayers.Contains(site.owner)) return (int)Likelihoods.Enemy;
                else if (client.allyPlayers.Contains(site.owner)) return (int)Likelihoods.Ally;
                else return (int)Likelihoods.Neutral;
            }
        }

        public static void ClearAllFactionMemberLikelihoods(FactionFile factionFile)
        {
            ServerClient[] clients = Network.Network.connectedClients.ToArray();
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

        public static void UpdateClientLikelihoods(ServerClient client)
        {
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            SiteFile[] sites = SiteManager.GetAllSites();

            StructureLikelihoodJSON structureLikelihoodJSON = new StructureLikelihoodJSON();

            foreach (SettlementFile settlement in settlements)
            {
                if (settlement.owner == client.username) continue;

                structureLikelihoodJSON.settlementTiles.Add(settlement.tile);
                structureLikelihoodJSON.settlementLikelihoods.Add(GetSettlementLikelihood(client, settlement).ToString());
            }

            foreach (SiteFile site in sites)
            {
                structureLikelihoodJSON.siteTiles.Add(site.tile);
                structureLikelihoodJSON.siteLikelihoods.Add(GetSiteLikelihood(client, site).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON("LikelihoodPacket", structureLikelihoodJSON);
            client.clientListener.SendData(packet);
        }
    }
}
