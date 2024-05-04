﻿using Shared;
using System.Reflection.Metadata.Ecma335;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OnlineFactionManager
    {
        public static void ParseFactionPacket(ServerClient client, Packet packet)
        {
            PlayerFactionData factionManifest = (PlayerFactionData)Serializer.ConvertBytesToObject(packet.contents);

            switch(factionManifest.manifestMode)
            {
                case FactionManifestMode.Create:
                    CreateFaction(client, factionManifest);
                    break;

                case FactionManifestMode.Delete:
                    DeleteFaction(client, factionManifest);
                    break;

                case FactionManifestMode.AddMember:
                    AddMemberToFaction(client, factionManifest);
                    break;

                case FactionManifestMode.RemoveMember:
                    RemoveMemberFromFaction(client, factionManifest);
                    break;

                case FactionManifestMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, factionManifest);
                    break;

                case FactionManifestMode.Promote:
                    PromoteMember(client, factionManifest);
                    break;

                case FactionManifestMode.Demote:
                    DemoteMember(client, factionManifest);
                    break;

                case FactionManifestMode.MemberList:
                    SendFactionMemberList(client, factionManifest);
                    break;
            }
        }

        private static FactionFile[] GetAllFactions()
        {
            List<FactionFile> factionFiles = new List<FactionFile>();

            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach(string faction in factions)
            {
                if (!faction.EndsWith(".json")) continue;
                factionFiles.Add(Serializer.SerializeFromFile<FactionFile>(faction));
            }

            return factionFiles.ToArray();
        }

        public static FactionFile GetFactionFromClient(ServerClient client)
        {
            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(".json")) continue;
                FactionFile factionFile = Serializer.SerializeFromFile<FactionFile>(faction);
                if (factionFile.factionName == client.factionName) return factionFile;
            }

            return null;
        }

        public static FactionFile GetFactionFromFactionName(string factionName)
        {
            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(".json")) continue;
                FactionFile factionFile = Serializer.SerializeFromFile<FactionFile>(faction);
                if (factionFile.factionName == factionName) return factionFile;
                
            }

            return null;
        }

        public static bool CheckIfUserIsInFaction(FactionFile factionFile, string usernameToCheck)
        {
            foreach(string str in factionFile.factionMembers)
            {
                if (str == usernameToCheck) return true;
            }

            return false;
        }

        public static FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for(int i = 0; i < factionFile.factionMembers.Count(); i++)
            {
                if (factionFile.factionMembers[i] == usernameToCheck)
                {
                    return factionFile.factionMemberRanks[i];
                }
            }

            return  FactionRanks.Member;
        }

        public static void SaveFactionFile(FactionFile factionFile)
        {
            string savePath = Path.Combine(Master.factionsPath, factionFile.factionName + ".json");
            Serializer.SerializeToFile(savePath, factionFile);
        }

        private static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            FactionFile[] factions = GetAllFactions();
            foreach(FactionFile faction in factions)
            {
                if (faction.factionName == nameToCheck) return true;
            }

            return false;
        }

        private static void CreateFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (CheckIfFactionExistsByName(factionManifest.manifestData))
            {
                factionManifest.manifestMode = FactionManifestMode.NameInUse;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                factionManifest.manifestMode = FactionManifestMode.Create;

                FactionFile factionFile = new FactionFile();
                factionFile.factionName = factionManifest.manifestData;
                factionFile.factionMembers.Add(client.username);
                factionFile.factionMemberRanks = factionFile.factionMemberRanks.Add(FactionRanks.Admin);
                SaveFactionFile(factionFile);

                client.hasFaction = true;
                client.factionName = factionFile.factionName;

                UserFile userFile = UserManager.GetUserFile(client);
                userFile.hasFaction = true;
                userFile.factionName = factionFile.factionName;
                UserManager.SaveUserFile(client, userFile);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);

                Logger.WriteToConsole($"[Created faction] > {client.username} > {factionFile.factionName}", Logger.LogMode.Warning);
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (!CheckIfFactionExistsByName(client.factionName)) return;
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }


            factionManifest.manifestMode = FactionManifestMode.Delete;

            UserFile[] userFiles = UserManager.GetAllUserFiles();
            foreach (UserFile userFile in userFiles)
            {
                if (userFile.factionName == client.factionName)
                {
                    userFile.hasFaction = false;
                    userFile.factionName = "";

                    UserManager.SaveUserFileFromName(userFile.username, userFile);
                }
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
            foreach (string str in factionFile.factionMembers)
            {
                ServerClient cClient = Network.connectedClients.ToList().Find(x => x.username == str);
                if (cClient != null)
                {
                    cClient.hasFaction = false;
                    cClient.factionName = "";
                    cClient.listener.EnqueuePacket(packet);

                            GoodwillManager.UpdateClientGoodwills(cClient);
                        }
                    }

            SiteFile[] factionSites = GetFactionSites(factionFile);
            foreach(SiteFile site in factionSites) SiteManager.DestroySiteFromFile(site);

            File.Delete(Path.Combine(Master.factionsPath, factionFile.factionName + ".json"));
            Logger.WriteToConsole($"[Deleted Faction] > {client.username} > {factionFile.factionName}", Logger.LogMode.Warning);
        }

        private static void AddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            ServerClient toAdd = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            if (factionFile == null) return;

            if (GetMemberRank(factionFile, client.username) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                return;
            }
            if ((toAdd == null) || toAdd.hasFaction || factionFile.factionMembers.Contains(toAdd.username)) 
                return;


            factionManifest.manifestData = factionFile.factionName;
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
            toAdd.listener.EnqueuePacket(packet);
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromFactionName(factionManifest.manifestData);

            if (factionFile == null) return;
            if (factionFile.factionMembers.Contains(client.username)) return;

            factionFile.factionMembers.Add(client.username);
            factionFile.factionMemberRanks = factionFile.factionMemberRanks.Add(FactionRanks.Member);
            SaveFactionFile(factionFile);

            client.hasFaction = true;
            client.factionName = factionFile.factionName;

            UserFile userFile = UserManager.GetUserFile(client);
            userFile.hasFaction = true;
            userFile.factionName = factionFile.factionName;
            UserManager.SaveUserFile(client, userFile);

            GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

            ServerClient[] members = GetAllConnectedFactionMembers(factionFile);
            foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile toRemoveLocal = UserManager.GetUserFileFromName(settlementFile.owner);
            ServerClient toRemove = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            FactionRanks playerRequestingRemovalRank = GetMemberRank(factionFile, client.username);
            FactionRanks playerToRemoveRank          = GetMemberRank(factionFile,settlementFile.owner);

            //If the admin is trying trying to remove themselves send the admin protection
            if ((playerRequestingRemovalRank == FactionRanks.Admin) && (settlementFile.owner == client.username))
            {
                factionManifest.manifestMode = FactionManifestMode.AdminProtection;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);
                return;
            }

            //If a player is not removing themselves and is a higher rank, then do the remove
            //else send no power
            if ((settlementFile.owner != client.username) || (playerToRemoveRank < playerRequestingRemovalRank))
                RemoveFromFaction();
            else
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);

            void RemoveFromFaction()
            {
                if (!factionFile.factionMembers.Contains(settlementFile.owner)) return;
                if (toRemove != null)
                {
                    toRemove.hasFaction = false;
                    toRemove.factionName = "";

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                    toRemove.listener.EnqueuePacket(packet);

                        GoodwillManager.UpdateClientGoodwills(toRemove);
                    }

                if (toRemoveLocal == null) return;
                toRemoveLocal.hasFaction = false;
                toRemoveLocal.factionName = "";
                UserManager.SaveUserFileFromName(toRemoveLocal.username, toRemoveLocal);

                for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                {
                    if (factionFile.factionMembers[i] == toRemoveLocal.username)
                    {
                        factionFile.factionMembers.RemoveAt(i);
                        factionFile.factionMemberRanks = factionFile.factionMemberRanks.RemoveAt(i);
                        SaveFactionFile(factionFile);
                        break;
                    }
                }

                ServerClient[] members = GetAllConnectedFactionMembers(factionFile);
                foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
            }
        }

        private static void PromoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                return;
            }
            if (!factionFile.factionMembers.Contains(userFile.username)) return;
            if (GetMemberRank(factionFile, settlementFile.owner) == FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                return;
            }

            
            for (int c = 0; c < factionFile.factionMembers.Count(); c++)
            {
                if (factionFile.factionMembers[c] == userFile.username)
                {
                    factionFile.factionMemberRanks[c] += 1;
                    SaveFactionFile(factionFile);
                    break;
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                return;
            }
            if (!factionFile.factionMembers.Contains(userFile.username)) return;

            for (int i = 0; i < factionFile.factionMembers.Count(); i++)
            {
                if (factionFile.factionMembers[i] == userFile.username)
                {
                    factionFile.factionMemberRanks[i] -= 1;
                    SaveFactionFile(factionFile);
                    break;
                }
            }
        }

        private static SiteFile[] GetFactionSites(FactionFile factionFile)
        {
            SiteFile[] sites = SiteManager.GetAllSites();
            List<SiteFile> factionSites = new List<SiteFile>();
            foreach(SiteFile site in sites)
            {
                if (site.isFromFaction && site.factionName == factionFile.factionName)
                {
                    factionSites.Add(site);
                }
            }

            return factionSites.ToArray();
        }

        private static ServerClient[] GetAllConnectedFactionMembers(FactionFile factionFile)
        {
            List<ServerClient> connectedFactionMembers = new List<ServerClient>();
            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                if (factionFile.factionMembers.Contains(client.username))
                {
                    connectedFactionMembers.Add(client);
                }
            }

            return connectedFactionMembers.ToArray();
        }

        private static void SendFactionMemberList(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);

            foreach(string str in factionFile.factionMembers)
            {
                factionManifest.manifestComplexData.Add(str);
                factionManifest.manifestSecondaryComplexData.Add(((int)GetMemberRank(factionFile, str)).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
            client.listener.EnqueuePacket(packet);
        }
    }
}
