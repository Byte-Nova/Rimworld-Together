using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class GuildManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionConfigs.EnableFactions)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PlayerGuildData factionManifest = Serializer.ConvertBytesToObject<PlayerGuildData>(packet.contents);

            switch (factionManifest._stepMode)
            {
                case GuildStepMode.Create:
                    CreateFaction(client, factionManifest);
                    break;

                case GuildStepMode.Delete:
                    DeleteFaction(client, factionManifest);
                    break;

                case GuildStepMode.AddMember:
                    AddMemberToFaction(client, factionManifest);
                    break;

                case GuildStepMode.RemoveMember:
                    RemoveMemberFromFaction(client, factionManifest);
                    break;

                case GuildStepMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, factionManifest);
                    break;

                case GuildStepMode.Promote:
                    PromoteMember(client, factionManifest);
                    break;

                case GuildStepMode.Demote:
                    DemoteMember(client, factionManifest);
                    break;

                case GuildStepMode.MemberList:
                    SendFactionMemberList(client, factionManifest);
                    break;
            }
        }

        private static void CreateFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            if (GuildManagerH.CheckIfFactionExistsByName(factionManifest._file.Name))
            {
                factionManifest._stepMode = GuildStepMode.NameInUse;

                Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                factionManifest._stepMode = GuildStepMode.Create;

                GuildFile factionFile = new GuildFile();
                factionFile.Name = factionManifest._file.Name;
                factionFile.CurrentUids.Add(client.userFile.Uid);
                factionFile.CurrentLabels.Add(client.userFile.Label);
                factionFile.CurrentRanks.Add((int)FactionRanks.Admin);
                GuildManagerH.SaveFactionFile(factionFile);

                foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(client.userFile.Uid))
                {
                    SiteManagerHelper.UpdateFaction(site, factionFile);
                }

                Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                client.listener.EnqueuePacket(packet);

                InformationDisplayer.DisplayAddFaction(factionFile.Name);
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            if (!GuildManagerH.CheckIfFactionExistsByName(client.userFile.GuildName)) return;
            else
            {
                GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);

                if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest._stepMode = GuildStepMode.Delete;

                    UserFile[] toUpdateOffline = GuildManagerH.GetUsersFromFactionMembers(factionFile);
                    foreach (UserFile userFile in toUpdateOffline) userFile.UpdateFaction(null);

                    Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                    SiteFile[] factionSites = GuildManagerH.GetFactionSites(factionFile);

                    foreach (SiteFile site in factionSites)
                    {
                        SiteManagerHelper.UpdateFaction(site, null);
                    }

                    foreach (ServerClient toUpdateConnected in GuildManagerH.GetConnectedFactionMembers(factionFile))
                    {
                        toUpdateConnected.userFile.UpdateFaction(null);
                        toUpdateConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
                    }
                    File.Delete(Path.Combine(Master.factionsPath, factionFile.Name + GuildManagerH.fileExtension));

                    InformationDisplayer.DisplayRemoveFaction(factionFile.Name);
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            ServerClient toAdd = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);

            if (factionFile == null) return;
            if (toAdd == null) return;

            if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) == FactionRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            else
            {
                if (!string.IsNullOrEmpty(toAdd.userFile.GuildName)) return;
                else
                {
                    if (factionFile.CurrentUids.Contains(toAdd.userFile.Uid)) return;
                    else
                    {
                        factionManifest._file.Name = factionFile.Name;
                        Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                        toAdd.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(factionManifest._file.Name);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.CurrentUids.Contains(client.userFile.Uid))
                {
                    factionFile.CurrentUids.Add(client.userFile.Uid);
                    factionFile.CurrentLabels.Add(client.userFile.Label);
                    factionFile.CurrentRanks.Add((int)FactionRanks.Member);
                    GuildManagerH.SaveFactionFile(factionFile);

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(client.userFile.Uid))
                    {
                        SiteManagerHelper.UpdateFaction(site, factionFile);
                    }

                    ServerClient[] members = GuildManagerH.GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile toUpdateOffline = UserManagerH.GetUserFileFromName(settlementFile.UID);
            ServerClient toRemoveConnected = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);

            if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) == FactionRanks.Member)
            {
                if (settlementFile.UID == client.userFile.Uid) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) == FactionRanks.Moderator)
            {
                if (settlementFile.UID == client.userFile.Uid) RemoveFromFaction();
                else
                {
                    if (GuildManagerH.GetMemberRank(factionFile, settlementFile.UID) != FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                    }
                    else RemoveFromFaction();
                }
            }

            else if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) == FactionRanks.Admin)
            {
                if (settlementFile.UID == client.userFile.Uid)
                {
                    factionManifest._stepMode = GuildStepMode.AdminProtection;
                    Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                    client.listener.EnqueuePacket(packet);
                }
                else RemoveFromFaction();
            }

            void RemoveFromFaction()
            {
                if (!factionFile.CurrentUids.Contains(settlementFile.UID)) return;
                else
                {
                    if (toRemoveConnected != null)
                    {
                        toRemoveConnected.userFile.UpdateFaction(null);
                        foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(toRemoveConnected.userFile.Uid))
                        {
                            SiteManagerHelper.UpdateFaction(site, null);
                        }

                        Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
                        toRemoveConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toRemoveConnected);
                    }

                    if (toUpdateOffline == null) return;
                    else
                    {
                        toUpdateOffline.UpdateFaction(null);
                        foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(toUpdateOffline.Uid))
                        {
                            SiteManagerHelper.UpdateFaction(site, null);
                        }
                        for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                        {
                            if (factionFile.CurrentUids[i] == toUpdateOffline.Uid)
                            {
                                factionFile.CurrentUids.RemoveAt(i);
                                factionFile.CurrentLabels.RemoveAt(i);
                                factionFile.CurrentRanks.RemoveAt(i);
                                GuildManagerH.SaveFactionFile(factionFile);
                                break;
                            }
                        }
                    }
                    ServerClient[] members = GuildManagerH.GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void PromoteMember(ServerClient client, PlayerGuildData factionManifest)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerH.GetUserFileFromName(settlementFile.UID);
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);

            if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GuildManagerH.GetMemberRank(factionFile, settlementFile.UID) != FactionRanks.Member && GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.CurrentUids.Contains(userFile.Uid)) return;
                else
                {
                    for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                    {
                        if (factionFile.CurrentUids[i] == userFile.Uid)
                        {
                            factionFile.CurrentRanks[i] = 1;
                            GuildManagerH.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerGuildData factionManifest)
        {
            SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerH.GetUserFileFromName(settlementFile.UID);
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);

            if (GuildManagerH.GetMemberRank(factionFile, client.userFile.Uid) != FactionRanks.Admin)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.CurrentUids.Contains(userFile.Uid)) return;
                else
                {
                    for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
                    {
                        if (factionFile.CurrentUids[i] == userFile.Uid)
                        {
                            factionFile.CurrentRanks[i] = 0;
                            GuildManagerH.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void SendFactionMemberList(ServerClient client, PlayerGuildData factionManifest)
        {
            factionManifest._file = GuildManagerH.GetFactionFromFactionName(client.userFile.GuildName);
            Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), factionManifest);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class GuildManagerH
    {
        //Variables

        public readonly static string fileExtension = ".mpfaction";

        public static void SaveFactionFile(GuildFile factionFile)
        {
            factionFile.SavingSemaphore.WaitOne();

            try
            {
                string savePath = Path.Combine(Master.factionsPath, factionFile.Name + fileExtension);
                Serializer.SerializeToFile(savePath, factionFile);

                foreach (string str in factionFile.CurrentUids)
                {
                    ServerClient toUpdateConnected = NetworkHelper.GetConnectedClientFromUid(str);
                    toUpdateConnected?.userFile.UpdateFaction(factionFile);

                    UserFile toUpdateOffline = UserManagerH.GetUserFileFromName(str);
                    toUpdateOffline?.UpdateFaction(factionFile);
                }

                SiteFile[] factionSites = GetFactionSites(factionFile);
                foreach (SiteFile site in factionSites) SiteManagerHelper.UpdateFaction(site, factionFile);
            }
            catch (Exception e) { Printer.Error(e.ToString()); }

            factionFile.SavingSemaphore.Release();
        }

        public static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            GuildFile factionFile = GetAllFactions().FirstOrDefault(fetch => fetch.Name == nameToCheck);
            if (factionFile != null) return true;
            else return false;
        }

        public static GuildFile[] GetAllFactions()
        {
            List<GuildFile> factionFiles = new List<GuildFile>();

            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(fileExtension)) continue;
                factionFiles.Add(Serializer.SerializeFromFile<GuildFile>(faction));
            }

            return factionFiles.ToArray();
        }

        public static GuildFile GetFactionFromFactionName(string factionName)
        {
            string[] factions = Directory.GetFiles(Master.factionsPath);
            foreach (string faction in factions)
            {
                GuildFile factionFile = Serializer.SerializeFromFile<GuildFile>(faction);
                if (factionFile.Name == factionName) return factionFile;
            }

            return new GuildFile();
        }

        public static bool CheckIfUserIsInFaction(GuildFile factionFile, string usernameToCheck)
        {
            if (factionFile.CurrentUids.Contains(usernameToCheck)) return true;
            else return false;
        }

        public static FactionRanks GetMemberRank(GuildFile factionFile, string usernameToCheck)
        {
            for (int i = 0; i < factionFile.CurrentUids.Count(); i++)
            {
                if (factionFile.CurrentUids[i] == usernameToCheck)
                {
                    return (FactionRanks)factionFile.CurrentRanks[i];
                }
            }

            return FactionRanks.Member;
        }

        public static SiteFile[] GetFactionSites(GuildFile factionFile)
        {
            return SiteManagerHelper.GetAllSites().Where(fetch => fetch.GuildName == factionFile.Name).ToArray();
        }

        public static ServerClient[] GetConnectedFactionMembers(GuildFile factionFile)
        {
            return NetworkHelper.GetConnectedClientsSafe().Where(fetch => fetch.userFile.GuildName == factionFile.Name).ToArray();
        }

        public static UserFile[] GetUsersFromFactionMembers(GuildFile factionFile)
        {
            return UserManagerH.GetAllUserFiles().Where(fetch => fetch.GuildName == factionFile.Name).ToArray();
        }
    }
}
