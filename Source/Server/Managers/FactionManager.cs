using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class FactionManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableFactions)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PlayerFactionData factionManifest = Serializer.ConvertBytesToObject<PlayerFactionData>(packet.contents);

            switch(factionManifest._stepMode)
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

        private static void CreateFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (FactionManagerHelper.CheckIfFactionExistsByName(factionManifest._factionFile.Name))
            {
                factionManifest._stepMode = FactionStepMode.NameInUse;

                Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                client.listener.EnqueuePacket(packet);
            }
            else
            {
                factionManifest._stepMode = FactionStepMode.Create;

                FactionFile factionFile = new FactionFile();
                factionFile.Name = factionManifest._factionFile.Name;
                factionFile.CurrentMembers.Add(client.userFile.Username);
                factionFile.CurrentRanks.Add((int)FactionRanks.Admin);
                FactionManagerHelper.SaveFactionFile(factionFile);

                foreach (SiteIdendityFile site in SiteManagerHelper.GetAllSitesFromUsername(client.userFile.Username))
                {
                    SiteManagerHelper.UpdateFaction(site, factionFile);
                }
                
                Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                client.listener.EnqueuePacket(packet);

                Logger.Warning($"[Created faction] > {client.userFile.Username} > {factionFile.Name}");
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (!FactionManagerHelper.CheckIfFactionExistsByName(client.userFile.FactionFile.Name)) return;
            else
            {
                FactionFile factionFile = client.userFile.FactionFile;

                if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest._stepMode = FactionStepMode.Delete;

                    UserFile[] toUpdateOffline = FactionManagerHelper.GetUsersFromFactionMembers(factionFile);
                    foreach (UserFile userFile in toUpdateOffline) userFile.UpdateFaction(null);

                    Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                    SiteIdendityFile[] factionSites = FactionManagerHelper.GetFactionSites(factionFile);
                    foreach (SiteIdendityFile site in factionSites)
                    {
                        SiteManagerHelper.UpdateFaction(site, null);
                    }
                    foreach (ServerClient toUpdateConnected in FactionManagerHelper.GetConnectedFactionMembers(factionFile))
                    {
                        toUpdateConnected.userFile.UpdateFaction(null);
                        toUpdateConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
                    }
                    File.Delete(Path.Combine(Master.factionsPath, factionFile.Name + FactionManagerHelper.fileExtension));
                    Logger.Warning($"[Deleted Faction] > {client.userFile.Username} > {factionFile.Name}");
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.FactionFile;
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            ServerClient toAdd = NetworkHelper.GetConnectedClientFromUsername(settlementFile.Owner);

            if (factionFile == null) return;
            if (toAdd == null) return;

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            else
            {
                if (toAdd.userFile.FactionFile != null) return;
                else
                {
                    if (factionFile.CurrentMembers.Contains(toAdd.userFile.Username)) return;
                    else
                    {
                        factionManifest._factionFile.Name = factionFile.Name;
                        Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                        toAdd.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = FactionManagerHelper.GetFactionFromFactionName(factionManifest._factionFile.Name);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.CurrentMembers.Contains(client.userFile.Username))
                {
                    factionFile.CurrentMembers.Add(client.userFile.Username);
                    factionFile.CurrentRanks.Add((int)FactionRanks.Member);
                    FactionManagerHelper.SaveFactionFile(factionFile);

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    foreach (SiteIdendityFile site in SiteManagerHelper.GetAllSitesFromUsername(client.userFile.Username))
                    {
                        SiteManagerHelper.UpdateFaction(site,factionFile);
                    }

                    ServerClient[] members = FactionManagerHelper.GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.FactionFile;
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile toUpdateOffline = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);
            ServerClient toRemoveConnected = NetworkHelper.GetConnectedClientFromUsername(settlementFile.Owner);

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
            {
                if (settlementFile.Owner == client.userFile.Username) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Moderator)
            {
                if (settlementFile.Owner == client.userFile.Username) RemoveFromFaction();
                else
                {
                    if (FactionManagerHelper.GetMemberRank(factionFile, settlementFile.Owner) != FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                    }
                    else RemoveFromFaction();
                }
            }

            else if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Admin)
            {
                if (settlementFile.Owner == client.userFile.Username)
                {
                    factionManifest._stepMode = FactionStepMode.AdminProtection;
                    Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                    client.listener.EnqueuePacket(packet);
                }
                else RemoveFromFaction();
            }

            void RemoveFromFaction()
            {
                if (!factionFile.CurrentMembers.Contains(settlementFile.Owner)) return;
                else
                {
                    if (toRemoveConnected != null)
                    {
                        toRemoveConnected.userFile.UpdateFaction(null);
                        foreach (SiteIdendityFile site in SiteManagerHelper.GetAllSitesFromUsername(toRemoveConnected.userFile.Username))
                        {
                            SiteManagerHelper.UpdateFaction(site, null);
                        }

                        Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
                        toRemoveConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toRemoveConnected);
                    }

                    if (toUpdateOffline == null) return;
                    else
                    {
                        toUpdateOffline.UpdateFaction(null);
                        foreach (SiteIdendityFile site in SiteManagerHelper.GetAllSitesFromUsername(toUpdateOffline.Username))
                        {
                            SiteManagerHelper.UpdateFaction(site, null);
                        }
                        for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
                        {
                            if (factionFile.CurrentMembers[i] == toUpdateOffline.Username)
                            {
                                factionFile.CurrentMembers.RemoveAt(i);
                                factionFile.CurrentRanks.RemoveAt(i);
                                FactionManagerHelper.SaveFactionFile(factionFile);
                                break;
                            }
                        }
                    }
                    ServerClient[] members = FactionManagerHelper.GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void PromoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);
            FactionFile factionFile = client.userFile.FactionFile;

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (FactionManagerHelper.GetMemberRank(factionFile, settlementFile.Owner) != FactionRanks.Member && FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.CurrentMembers.Contains(userFile.Username)) return;
                else
                {
                    for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
                    {
                        if (factionFile.CurrentMembers[i] == userFile.Username)
                        {
                            factionFile.CurrentRanks[i] = 1;
                            FactionManagerHelper.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);
            FactionFile factionFile = client.userFile.FactionFile;

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.CurrentMembers.Contains(userFile.Username)) return;
                else
                {
                    for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
                    {
                        if (factionFile.CurrentMembers[i] == userFile.Username)
                        {
                            factionFile.CurrentRanks[i] = 0;
                            FactionManagerHelper.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void SendFactionMemberList(ServerClient client, PlayerFactionData factionManifest)
        {
            factionManifest._factionFile = client.userFile.FactionFile;
            Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), factionManifest);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class FactionManagerHelper
    {
        //Variables

        public readonly static string fileExtension = ".mpfaction";

        public static void SaveFactionFile(FactionFile factionFile)
        {
            factionFile.SavingSemaphore.WaitOne();

            try
            {
                string savePath = Path.Combine(Master.factionsPath, factionFile.Name + fileExtension);
                Serializer.SerializeToFile(savePath, factionFile);

                foreach (string str in factionFile.CurrentMembers)
                {
                    ServerClient toUpdateConnected = NetworkHelper.GetConnectedClientFromUsername(str);
                    toUpdateConnected?.userFile.UpdateFaction(factionFile);

                    UserFile toUpdateOffline = UserManagerHelper.GetUserFileFromName(str);
                    toUpdateOffline?.UpdateFaction(factionFile);
                }

                SiteIdendityFile[] factionSites = GetFactionSites(factionFile);
                foreach(SiteIdendityFile site in factionSites) SiteManagerHelper.UpdateFaction(site, factionFile);
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            factionFile.SavingSemaphore.Release();
        }

        public static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            FactionFile factionFile = GetAllFactions().FirstOrDefault(fetch => fetch.Name == nameToCheck);
            if (factionFile != null) return true;
            else return false;
        }

        public static FactionFile[] GetAllFactions()
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
                if (factionFile.Name == factionName) return factionFile;
            }

            return null;
        }

        public static bool CheckIfUserIsInFaction(FactionFile factionFile, string usernameToCheck)
        {
            if (factionFile.CurrentMembers.Contains(usernameToCheck)) return true;
            else return false;
        }

        public static FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for (int i = 0; i < factionFile.CurrentMembers.Count(); i++)
            {
                if (factionFile.CurrentMembers[i] == usernameToCheck)
                {
                    return (FactionRanks)factionFile.CurrentRanks[i];
                }
            }

            return FactionRanks.Member;
        }

        public static SiteIdendityFile[] GetFactionSites(FactionFile factionFile)
        {
            return SiteManagerHelper.GetAllSites().Where(fetch => fetch.FactionFile != null && 
                fetch.FactionFile.Name == factionFile.Name).ToArray();
        }

        public static ServerClient[] GetConnectedFactionMembers(FactionFile factionFile)
        {
            return NetworkHelper.GetConnectedClientsSafe().Where(fetch => fetch.userFile.FactionFile != null && 
                fetch.userFile.FactionFile.Name == factionFile.Name).ToArray();
        }

        public static UserFile[] GetUsersFromFactionMembers(FactionFile factionFile)
        {
            return UserManagerHelper.GetAllUserFiles().Where(fetch => fetch.FactionFile != null && 
                fetch.FactionFile.Name == factionFile.Name).ToArray();
        }
    }
}
