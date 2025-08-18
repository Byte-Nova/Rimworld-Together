using GameServer.Core;
using GameServer.Files;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Server;

namespace GameServer.Managers
{

    public static class GuildManager
    {
        [HandlesPacket(PacketHeader.GuildManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableFactions)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PlayerGuildData data = Serializer.ConvertBytesToObject<PlayerGuildData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case GuildStepMode.Create:
                    CreateFaction(client, data);
                    break;

                case GuildStepMode.Delete:
                    DeleteFaction(client, data);
                    break;

                case GuildStepMode.AddMember:
                    AddMemberToFaction(client, data);
                    break;

                case GuildStepMode.RemoveMember:
                    RemoveMemberFromFaction(client, data);
                    break;

                case GuildStepMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, data);
                    break;

                case GuildStepMode.Promote:
                    PromoteMember(client, data);
                    break;

                case GuildStepMode.Demote:
                    DemoteMember(client, data);
                    break;

                case GuildStepMode.MemberList:
                    SendFactionMemberList(client, data);
                    break;
            }
        }

        private static void CreateFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            if (GuildManagerH.CheckIfFactionExistsByName(factionManifest._file.Name))
            {
                factionManifest._stepMode = GuildStepMode.NameInUse;

                client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
            }

            else
            {
                factionManifest._stepMode = GuildStepMode.Create;

                GuildFile factionFile = new GuildFile();
                factionFile.Name = factionManifest._file.Name;
                factionFile.CurrentUids.Add(client.UserFile.Uid);
                factionFile.CurrentLabels.Add(client.UserFile.Label);
                factionFile.CurrentRanks.Add((int)FactionRanks.Admin);
                GuildManagerH.SaveFactionFile(factionFile);

                foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(client.UserFile.Uid))
                {
                    SiteManagerHelper.UpdateFaction(site, factionFile);
                }

                client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);

                InformationDisplayer.DisplayAddFaction(factionFile.Name);
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            if (!GuildManagerH.CheckIfFactionExistsByName(client.UserFile.GuildName)) return;
            else
            {
                GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);

                if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest._stepMode = GuildStepMode.Delete;

                    UserFile[] toUpdateOffline = GuildManagerH.GetUsersFromFactionMembers(factionFile);
                    foreach (UserFile userFile in toUpdateOffline) userFile.UpdateFaction(null);

                    SiteFile[] factionSites = GuildManagerH.GetFactionSites(factionFile);

                    foreach (SiteFile site in factionSites)
                    {
                        SiteManagerHelper.UpdateFaction(site, null);
                    }

                    foreach (ServerClient toUpdateConnected in GuildManagerH.GetConnectedFactionMembers(factionFile))
                    {
                        toUpdateConnected.UserFile.UpdateFaction(null);
                        toUpdateConnected.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
                        GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
                    }
                    File.Delete(Path.Combine(Master.FactionsPath, factionFile.Name + GuildManagerH.fileExtension));

                    InformationDisplayer.DisplayRemoveFaction(factionFile.Name);
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            ServerClient toAdd = ServerNetwork.Instance.GetConnectedClientFromUid(settlementFile.UID);

            if (factionFile == null) return;
            if (toAdd == null) return;

            if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) == FactionRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            else
            {
                if (!string.IsNullOrEmpty(toAdd.UserFile.GuildName)) return;
                else
                {
                    if (factionFile.CurrentUids.Contains(toAdd.UserFile.Uid)) return;
                    else
                    {
                        factionManifest._file.Name = factionFile.Name;
                        toAdd.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
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
                if (!factionFile.CurrentUids.Contains(client.UserFile.Uid))
                {
                    factionFile.CurrentUids.Add(client.UserFile.Uid);
                    factionFile.CurrentLabels.Add(client.UserFile.Label);
                    factionFile.CurrentRanks.Add((int)FactionRanks.Member);
                    GuildManagerH.SaveFactionFile(factionFile);

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(client.UserFile.Uid))
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
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile toUpdateOffline = UserManagerH.GetUserFileFromName(settlementFile.UID);
            ServerClient toRemoveConnected = ServerNetwork.Instance.GetConnectedClientFromUid(settlementFile.UID);

            if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) == FactionRanks.Member)
            {
                if (settlementFile.UID == client.UserFile.Uid) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) == FactionRanks.Moderator)
            {
                if (settlementFile.UID == client.UserFile.Uid) RemoveFromFaction();
                else
                {
                    if (GuildManagerH.GetMemberRank(factionFile, settlementFile.UID) != FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                    }
                    else RemoveFromFaction();
                }
            }

            else if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) == FactionRanks.Admin)
            {
                if (settlementFile.UID == client.UserFile.Uid)
                {
                    factionManifest._stepMode = GuildStepMode.AdminProtection;
                    client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
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
                        toRemoveConnected.UserFile.UpdateFaction(null);
                        foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUID(toRemoveConnected.UserFile.Uid))
                        {
                            SiteManagerHelper.UpdateFaction(site, null);
                        }

                        toRemoveConnected.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
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
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerH.GetUserFileFromName(settlementFile.UID);
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);

            if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GuildManagerH.GetMemberRank(factionFile, settlementFile.UID) != FactionRanks.Member && GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) != FactionRanks.Admin)
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
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            UserFile userFile = UserManagerH.GetUserFileFromName(settlementFile.UID);
            GuildFile factionFile = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);

            if (GuildManagerH.GetMemberRank(factionFile, client.UserFile.Uid) != FactionRanks.Admin)
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
            factionManifest._file = GuildManagerH.GetFactionFromFactionName(client.UserFile.GuildName);
            client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
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
                string savePath = Path.Combine(Master.FactionsPath, factionFile.Name + fileExtension);
                Serializer.SerializeToFile(savePath, factionFile);

                foreach (string str in factionFile.CurrentUids)
                {
                    ServerClient toUpdateConnected = ServerNetwork.Instance.GetConnectedClientFromUid(str);
                    toUpdateConnected?.UserFile.UpdateFaction(factionFile);

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

            string[] factions = Directory.GetFiles(Master.FactionsPath);
            foreach (string faction in factions)
            {
                if (!faction.EndsWith(fileExtension)) continue;
                factionFiles.Add(Serializer.SerializeFromFile<GuildFile>(faction));
            }

            return factionFiles.ToArray();
        }

        public static GuildFile GetFactionFromFactionName(string factionName)
        {
            string[] factions = Directory.GetFiles(Master.FactionsPath);
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
            return ServerNetwork.Instance.GetConnectedClientsSafe().Where(fetch => fetch.UserFile.GuildName == factionFile.Name).ToArray();
        }

        public static UserFile[] GetUsersFromFactionMembers(GuildFile factionFile)
        {
            return UserManagerH.GetAllUserFiles().Where(fetch => fetch.GuildName == factionFile.Name).ToArray();
        }
    }
}
