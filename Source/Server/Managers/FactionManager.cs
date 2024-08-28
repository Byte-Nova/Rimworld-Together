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

            foreach (string str in factionFile.currentMembers)
            {
                ServerClient toUpdateConnected = UserManagerHelper.GetConnectedClientFromUsername(str);
                toUpdateConnected?.userFile.UpdateFaction(factionFile);

                UserFile toUpdateOffline = UserManagerHelper.GetUserFileFromName(str);
                toUpdateOffline?.UpdateFaction(factionFile);
            }
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
                FactionFile factionFile = client.userFile.faction;

                if (GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest.stepMode = FactionStepMode.Delete;

                    UserFile[] toUpdateOffline = GetUsersFromFactionMembers(factionFile);
                    foreach (UserFile userFile in toUpdateOffline) userFile.UpdateFaction(null);

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                    foreach (ServerClient toUpdateConnected in GetConnectedFactionMembers(factionFile))
                    {
                        toUpdateConnected.userFile.UpdateFaction(null);
                        toUpdateConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
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
            FactionFile factionFile = client.userFile.faction;
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

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    ServerClient[] members = GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.faction;
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            UserFile toUpdateOffline = UserManagerHelper.GetUserFileFromName(settlementFile.owner);
            ServerClient toRemoveConnected = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);

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
                    if (toRemoveConnected != null)
                    {
                        toRemoveConnected.userFile.UpdateFaction(null);

                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                        toRemoveConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toRemoveConnected);
                    }

                    if (toUpdateOffline == null) return;
                    else
                    {
                        toUpdateOffline.UpdateFaction(null);

                        for (int i = 0; i < factionFile.currentMembers.Count(); i++)
                        {
                            if (factionFile.currentMembers[i] == toUpdateOffline.Username)
                            {
                                factionFile.currentMembers.RemoveAt(i);
                                factionFile.currentRanks.RemoveAt(i);
                                SaveFactionFile(factionFile);
                                break;
                            }
                        }
                    }

                    ServerClient[] members = GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void PromoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = client.userFile.faction;

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
            FactionFile factionFile = client.userFile.faction;

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

        private static ServerClient[] GetConnectedFactionMembers(FactionFile factionFile)
        {
            return NetworkHelper.GetConnectedClientsSafe().Where(fetch => fetch.userFile.faction != null && 
                fetch.userFile.faction.name == factionFile.name).ToArray();
        }

        private static UserFile[] GetUsersFromFactionMembers(FactionFile factionFile)
        {
            return UserManagerHelper.GetAllUserFiles().Where(fetch => fetch.faction != null && 
                fetch.faction.name == factionFile.name).ToArray();
        }

        private static void SendFactionMemberList(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.faction;

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
