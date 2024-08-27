using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class FactionManager
    {
        //Variables

        public readonly static string fileExtension = ".mpfaction";

        public static void ParseFactionPacket(ServerClient client, Packet packet)
        {
            PlayerFactionData factionManifest = Serializer.ConvertBytesToObject<PlayerFactionData>(packet.contents);

            switch(factionManifest.stepMode)
            {
                case FactionStepMode.Create:
                    CreateFaction(client, factionManifest);
                    break;

                case FactionStepMode.Delete:
                    DeleteFaction(client, factionManifest);
                    break;

                case FactionStepMode.AddMember:
                    AddMemberToFaction(client, factionManifest);
                    break;

                case FactionStepMode.RemoveMember:
                    RemoveMemberFromFaction(client, factionManifest);
                    break;

                case FactionStepMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, factionManifest);
                    break;

                case FactionStepMode.Promote:
                    PromoteMember(client, factionManifest);
                    break;

                case FactionStepMode.Demote:
                    DemoteMember(client, factionManifest);
                    break;

                case FactionStepMode.MemberList:
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
                if (!faction.EndsWith(fileExtension)) continue;
                factionFiles.Add(Serializer.SerializeFromFile<FactionFile>(faction));
            }

            return factionFiles.ToArray();
        }

        public static FactionFile GetFactionFromClient(ServerClient client)
        {
            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(fileExtension)) continue;

                FactionFile factionFile = Serializer.SerializeFromFile<FactionFile>(faction);
                if (factionFile.name == client.userFile.faction.name) return factionFile;
            }

            return null;
        }

        public static FactionFile GetFactionFromFactionName(string factionName)
        {
            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(fileExtension)) continue;

                FactionFile factionFile = Serializer.SerializeFromFile<FactionFile>(faction);
                if (factionFile.name == factionName) return factionFile;
            }

            return null;
        }

        public static bool CheckIfUserIsInFaction(FactionFile factionFile, string usernameToCheck)
        {
            foreach(string str in factionFile.currentMembers)
            {
                if (str == usernameToCheck) return true;
            }

            return false;
        }

        public static FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for(int i = 0; i < factionFile.currentMembers.Count(); i++)
            {
                if (factionFile.currentMembers[i] == usernameToCheck)
                {
                    return (FactionRanks)factionFile.currentRanks[i];
                }
            }

            return FactionRanks.Member;
        }

        public static void SaveFactionFile(FactionFile factionFile)
        {
            string savePath = Path.Combine(Master.factionsPath, factionFile.name + fileExtension);
            Serializer.SerializeToFile(savePath, factionFile);
        }

        private static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            FactionFile[] factions = GetAllFactions();
            foreach(FactionFile faction in factions)
            {
                if (faction.name == nameToCheck) return true;
            }

            return false;
        }

        private static void CreateFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (CheckIfFactionExistsByName(factionManifest.manifestDataString))
            {
                factionManifest.stepMode = FactionStepMode.NameInUse;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                factionManifest.stepMode = FactionStepMode.Create;

                FactionFile factionFile = new FactionFile();
                factionFile.name = factionManifest.manifestDataString;
                factionFile.currentMembers.Add(client.userFile.Username);
                factionFile.currentRanks.Add((int)FactionRanks.Admin);
                SaveFactionFile(factionFile);

                client.userFile.UpdateFaction(factionFile);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);

                Logger.Warning($"[Created faction] > {client.userFile.Username} > {factionFile.name}");
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (!CheckIfFactionExistsByName(client.userFile.faction.name)) return;
            else
            {
                FactionFile factionFile = GetFactionFromClient(client);

                if (GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest.stepMode = FactionStepMode.Delete;

                    UserFile[] userFiles = UserManagerHelper.GetAllUserFiles();
                    foreach (UserFile userFile in userFiles)
                    {
                        if (userFile.faction.name == client.userFile.faction.name) userFile.UpdateFaction(null);
                    }

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                    foreach (string str in factionFile.currentMembers)
                    {
                        ServerClient cClient = Network.connectedClients.ToList().Find(x => x.userFile.Username == str);
                        if (cClient != null)
                        {
                            cClient.userFile.UpdateFaction(null);
                            cClient.listener.EnqueuePacket(packet);
                            GoodwillManager.UpdateClientGoodwills(cClient);
                        }
                    }

                    SiteFile[] factionSites = GetFactionSites(factionFile);
                    foreach(SiteFile site in factionSites) SiteManager.DestroySiteFromFile(site);

                    File.Delete(Path.Combine(Master.factionsPath, factionFile.name + fileExtension));
                    Logger.Warning($"[Deleted Faction] > {client.userFile.Username} > {factionFile.name}");
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            ServerClient toAdd = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);

            if (factionFile == null) return;
            if (toAdd == null) return;

            if (GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            else
            {
                if (toAdd.userFile.faction != null) return;
                else
                {
                    if (factionFile.currentMembers.Contains(toAdd.userFile.Username)) return;
                    else
                    {
                        factionManifest.manifestDataString = factionFile.name;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                        toAdd.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromFactionName(factionManifest.manifestDataString);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.currentMembers.Contains(client.userFile.Username))
                {
                    factionFile.currentMembers.Add(client.userFile.Username);
                    factionFile.currentRanks.Add((int)FactionRanks.Member);
                    SaveFactionFile(factionFile);

                    client.userFile.UpdateFaction(factionFile);

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    ServerClient[] members = GetAllConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            UserFile toRemoveLocal = UserManagerHelper.GetUserFileFromName(settlementFile.owner);
            ServerClient toRemove = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);

            if (GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
            {
                if (settlementFile.owner == client.userFile.Username) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Moderator)
            {
                if (settlementFile.owner == client.userFile.Username) RemoveFromFaction();
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) != FactionRanks.Member)
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);

                    else RemoveFromFaction();
                }
            }

            else if (GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Admin)
            {
                if (settlementFile.owner == client.userFile.Username)
                {
                    factionManifest.stepMode = FactionStepMode.AdminProtection;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                    client.listener.EnqueuePacket(packet);
                }
                else RemoveFromFaction();
            }

            void RemoveFromFaction()
            {
                if (!factionFile.currentMembers.Contains(settlementFile.owner)) return;
                else
                {
                    if (toRemove != null)
                    {
                        toRemove.userFile.UpdateFaction(null);

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                        toRemove.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toRemove);
                    }

                    if (toRemoveLocal == null) return;
                    else
                    {
                        toRemoveLocal.UpdateFaction(null);

                        for (int i = 0; i < factionFile.currentMembers.Count(); i++)
                        {
                            if (factionFile.currentMembers[i] == toRemoveLocal.Username)
                            {
                                factionFile.currentMembers.RemoveAt(i);
                                factionFile.currentRanks.RemoveAt(i);
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
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.currentMembers.Contains(userFile.Username)) return;
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) == FactionRanks.Admin)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                    }

                    else
                    {
                        for (int i = 0; i < factionFile.currentMembers.Count(); i++)
                        {
                            if (factionFile.currentMembers[i] == userFile.Username)
                            {
                                factionFile.currentRanks[i] = 1;
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
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.currentMembers.Contains(userFile.Username)) return;
                else
                {
                    for (int i = 0; i < factionFile.currentMembers.Count(); i++)
                    {
                        if (factionFile.currentMembers[i] == userFile.Username)
                        {
                            factionFile.currentRanks[i] = 0;
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
                if (site.factionFile != null && site.factionFile.name == factionFile.name)
                {
                    factionSites.Add(site);
                }
            }

            return factionSites.ToArray();
        }

        private static ServerClient[] GetAllConnectedFactionMembers(FactionFile factionFile)
        {
            List<ServerClient> connectedFactionMembers = new List<ServerClient>();
            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                if (factionFile.currentMembers.Contains(client.userFile.Username))
                {
                    connectedFactionMembers.Add(client);
                }
            }

            return connectedFactionMembers.ToArray();
        }

        private static void SendFactionMemberList(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);

            foreach(string str in factionFile.currentMembers)
            {
                factionManifest.manifestComplexData.Add(str);
                factionManifest.manifestSecondaryComplexData.Add(((int)GetMemberRank(factionFile, str)).ToString());
            }

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
            client.listener.EnqueuePacket(packet);
        }
    }
}
