using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OnlineFactionManager
    {
        public static void ParseFactionPacket(ServerClient client, Packet packet)
        {
            PlayerFactionData factionManifest = (PlayerFactionData)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(factionManifest.manifestMode))
            {
                case (int)CommonEnumerators.FactionManifestMode.Create:
                    CreateFaction(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.Delete:
                    DeleteFaction(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.AddMember:
                    AddMemberToFaction(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.RemoveMember:
                    RemoveMemberFromFaction(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.Promote:
                    PromoteMember(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.Demote:
                    DemoteMember(client, factionManifest);
                    break;

                case (int)CommonEnumerators.FactionManifestMode.MemberList:
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

        public static CommonEnumerators.FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for(int i = 0; i < factionFile.factionMembers.Count(); i++)
            {
                if (factionFile.factionMembers[i] == usernameToCheck)
                {
                    return (CommonEnumerators.FactionRanks)int.Parse(factionFile.factionMemberRanks[i]);
                }
            }

            return CommonEnumerators.FactionRanks.Member;
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
                factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NameInUse).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Create).ToString();

                FactionFile factionFile = new FactionFile();
                factionFile.factionName = factionManifest.manifestData;
                factionFile.factionMembers.Add(client.username);
                factionFile.factionMemberRanks.Add(((int)CommonEnumerators.FactionRanks.Admin).ToString());
                SaveFactionFile(factionFile);

                client.hasFaction = true;
                client.factionName = factionFile.factionName;

                UserFile userFile = UserManager.GetUserFile(client);
                userFile.hasFaction = true;
                userFile.factionName = factionFile.factionName;
                UserManager.SaveUserFile(client, userFile);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);

                ConsoleManager.WriteToConsole($"[Created faction] > {client.username} > {factionFile.factionName}", LogMode.Warning);
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (!CheckIfFactionExistsByName(client.factionName)) return;
            else
            {
                FactionFile factionFile = GetFactionFromClient(client);

                if (GetMemberRank(factionFile, client.username) != CommonEnumerators.FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Delete).ToString();

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
                    ConsoleManager.WriteToConsole($"[Deleted Faction] > {client.username} > {factionFile.factionName}", LogMode.Warning);
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            ServerClient toAdd = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            if (factionFile == null) return;
            else
            {
                if (GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Member)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    if (toAdd == null) return;
                    else
                    {
                        if (toAdd.hasFaction) return;
                        else
                        {
                            if (factionFile.factionMembers.Contains(toAdd.username)) return;
                            else
                            {
                                factionManifest.manifestData = factionFile.factionName;
                                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                                toAdd.listener.EnqueuePacket(packet);
                            }
                        }
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromFactionName(factionManifest.manifestData);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.factionMembers.Contains(client.username))
                {
                    factionFile.factionMembers.Add(client.username);
                    factionFile.factionMemberRanks.Add(((int)CommonEnumerators.FactionRanks.Member).ToString());
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
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile toRemoveLocal = UserManager.GetUserFileFromName(settlementFile.owner);
            ServerClient toRemove = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            if (GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Member)
            {
                if (settlementFile.owner == client.username) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Moderator)
            {
                if (settlementFile.owner == client.username) RemoveFromFaction();
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) != CommonEnumerators.FactionRanks.Member)
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);

                    else RemoveFromFaction();
                }
            }

            else if (GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Admin)
            {
                if (settlementFile.owner == client.username)
                {
                    factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.AdminProtection).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                    client.listener.EnqueuePacket(packet);
                }
                else RemoveFromFaction();
            }

            void RemoveFromFaction()
            {
                if (!factionFile.factionMembers.Contains(settlementFile.owner)) return;
                else
                {
                    if (toRemove != null)
                    {
                        toRemove.hasFaction = false;
                        toRemove.factionName = "";

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
                        toRemove.listener.EnqueuePacket(packet);

                        GoodwillManager.UpdateClientGoodwills(toRemove);
                    }

                    if (toRemoveLocal == null) return;
                    else
                    {
                        toRemoveLocal.hasFaction = false;
                        toRemoveLocal.factionName = "";
                        UserManager.SaveUserFileFromName(toRemoveLocal.username, toRemoveLocal);

                        for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                        {
                            if (factionFile.factionMembers[i] == toRemoveLocal.username)
                            {
                                factionFile.factionMembers.RemoveAt(i);
                                factionFile.factionMemberRanks.RemoveAt(i);
                                SaveFactionFile(factionFile);
                                break;
                            }
                        }
                    }

                    ServerClient[] members = GetAllConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void PromoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.factionMembers.Contains(userFile.username)) return;
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) == CommonEnumerators.FactionRanks.Admin)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                    }

                    else
                    {
                        for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                        {
                            if (factionFile.factionMembers[i] == userFile.username)
                            {
                                factionFile.factionMemberRanks[i] = "1";
                                SaveFactionFile(factionFile);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestData);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) != CommonEnumerators.FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.factionMembers.Contains(userFile.username)) return;
                else
                {
                    for (int i = 0; i < factionFile.factionMembers.Count(); i++)
                    {
                        if (factionFile.factionMembers[i] == userFile.username)
                        {
                            factionFile.factionMemberRanks[i] = "0";
                            SaveFactionFile(factionFile);
                            break;
                        }
                    }
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
