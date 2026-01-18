using GameServer.Core;
using GameServer.Hooks.TCPNetwork;
using GameServer.Misc;
using Shared;
using Shared.Files;
using Shared.Files.Guilds;
using Shared.Files.Sites;
using TCPNetwork;
using TCPNetwork.Files.Client;
using TCPNetwork.Packets;
using static Shared.CommonEnumerators;
using static Shared.Files.Guilds.GuildMember;

namespace GameServer.Managers
{

    public static class GuildManager
    {
        [HandlesPacket(PacketHeader.GuildManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.EnableFactions)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PlayerGuildData data = Serializer.ConvertBytesToObject<PlayerGuildData>(bytes);

            switch (data._stepMode)
            {
                case GuildStepMode.Create:
                    CreateFaction(client, data);
                    break;

                case GuildStepMode.Delete:
                    DeleteFaction(client, data);
                    break;

                case GuildStepMode.Invite:
                    InviteMemberToFaction(client, data);
                    break;

                case GuildStepMode.AddMember:
                    AddMemberToFaction(client, data);
                    break;

                case GuildStepMode.RemoveMember:
                    RemoveMemberFromFaction(client, data);
                    break;

                case GuildStepMode.Promote:
                    PromoteMember(client, data);
                    break;

                case GuildStepMode.Demote:
                    DemoteMember(client, data);
                    break;

                case GuildStepMode.MemberList:
                    SendMemberList(client, data);
                    break;
            }
        }

        private static void CreateFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            if (GuildManagerH.CheckIfFactionExistsByName(factionManifest._guild.Name))
            {
                factionManifest._stepMode = GuildStepMode.NameInUse;
                client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
            }

            else
            {
                factionManifest._stepMode = GuildStepMode.Create;

                GuildMember member = new GuildMember();
                member.Username = client.UserFile.Username;
                member.Rank = GuildMember.GuildRanks.Admin;

                GuildFile factionFile = new GuildFile();
                factionFile.Name = factionManifest._guild.Name;
                factionFile.AddMember(member);

                client.UserFile.UpdateFaction(factionFile);

                foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUsername(client.UserFile.Username)) site.UpdateFaction(factionFile);

                client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);

                InformationDisplayer.DisplayAddFaction(factionFile.Name);
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);

            if (GuildManagerH.GetMemberRank(guild, client.UserFile.Username) != GuildRanks.Admin) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                foreach (UserFile userFile in GuildManagerH.GetUsersFromFactionMembers(guild)) userFile.UpdateFaction(null);

                foreach (SiteFile site in GuildManagerH.GetFactionSites(guild)) site.UpdateFaction(null);

                factionManifest._stepMode = GuildStepMode.Delete;
                foreach (ServerClient toUpdateConnected in GuildManagerH.GetConnectedFactionMembers(guild))
                {
                    toUpdateConnected.UserFile.UpdateFaction(null);
                    toUpdateConnected.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
                    GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
                }

                guild.Delete();

                InformationDisplayer.DisplayRemoveFaction(guild.Name);
            }
        }

        private static void InviteMemberToFaction(ServerClient client, PlayerGuildData guildManifest)
        {
            GuildFile guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(guildManifest._dataInt);
            ServerClient toAdd = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

            if (GuildManagerH.GetMemberRank(guild, client.UserFile.Username) == GuildRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                if (toAdd.UserFile.GuildName != null) return;
                else
                {
                    guildManifest._guild.Name = guild.Name;
                    toAdd.Listener.EnqueuePacket(PacketHeader.GuildManager, guildManifest);
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerGuildData factionManifest)
        {
            GuildFile guild = GuildManagerH.GetFactionFromName(factionManifest._guild.Name);

            if (!GuildManagerH.CheckIfUserIsInFaction(guild, client.UserFile.Username))
            {
                GuildMember member = new GuildMember();
                member.Username = client.UserFile.Username;
                member.Rank = GuildRanks.Member;
                guild.AddMember(member);

                client.UserFile.UpdateFaction(guild);

                foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUsername(client.UserFile.Username)) site.UpdateFaction(guild);

                foreach (ServerClient sc in GuildManagerH.GetConnectedFactionMembers(guild)) GoodwillManager.UpdateClientGoodwills(sc);
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerGuildData guildManifest)
        {
            GuildFile guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(guildManifest._dataInt);
            UserFile toRemoveOffline = UserManagerH.GetUserFileFromName(settlement.Username);
            ServerClient toRemoveOnline = ServerNetwork.Instance.GetConnectedClientFromUsername(settlement.Username);

            GuildRanks userRank = GuildManagerH.GetMemberRank(guild, client.UserFile.Username);

            if (settlement.Username == client.UserFile.Username)
            {
                if (userRank != GuildRanks.Admin) Remove();
                else
                {
                    guildManifest._stepMode = GuildStepMode.AdminProtection;
                    client.Listener.EnqueuePacket(PacketHeader.GuildManager, guildManifest);
                }
            }

            else
            {
                if (userRank == GuildRanks.Member || userRank == GuildRanks.Moderator) ResponseShortcutManager.SendNoPowerPacket(client);
                else Remove();
            }

            void Remove()
            {
                if (toRemoveOnline != null)
                {
                    toRemoveOnline.UserFile.UpdateFaction(null);

                    GoodwillManager.UpdateClientGoodwills(toRemoveOnline);

                    toRemoveOnline.Listener.EnqueuePacket(PacketHeader.GuildManager, guildManifest);
                }

                toRemoveOffline.UpdateFaction(null);

                guild.RemoveMember(guild.GuildMembers.First(fetch => fetch.Username == toRemoveOffline.Username));

                foreach (SiteFile site in SiteManagerHelper.GetAllSitesFromUsername(toRemoveOffline.Username)) site.UpdateFaction(null);

                foreach (ServerClient member in GuildManagerH.GetConnectedFactionMembers(guild)) GoodwillManager.UpdateClientGoodwills(member);
            }
        }

        private static void PromoteMember(ServerClient client, PlayerGuildData factionManifest)
        {
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            GuildFile guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);

            GuildRanks rank = GuildManagerH.GetMemberRank(guild, client.UserFile.Username);
            if (rank == GuildRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                UserFile toPromoteOffline = UserManagerH.GetUserFileFromName(settlement.Username);
                if (GuildManagerH.GetMemberRank(guild, toPromoteOffline.Username) != GuildRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client);
                else
                {
                    GuildMember member = GuildManagerH.GetAllFactionMembers(guild).First(fetch => fetch.Username == toPromoteOffline.Username);
                    guild.PromoteMember(member);

                    ServerClient toPromoteOnline = ServerNetwork.Instance.GetConnectedClientFromUsername(toPromoteOffline.Username);
                    if (toPromoteOnline != null) toPromoteOnline.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerGuildData factionManifest)
        {
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(factionManifest._dataInt);
            GuildFile guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);

            GuildRanks rank = GuildManagerH.GetMemberRank(guild, client.UserFile.Username);
            if (rank != GuildRanks.Admin) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                UserFile toDemoteOffline = UserManagerH.GetUserFileFromName(settlement.Username);
                if (GuildManagerH.GetMemberRank(guild, toDemoteOffline.Username) != GuildRanks.Moderator) ResponseShortcutManager.SendNoPowerPacket(client);
                else
                {
                    GuildMember member = GuildManagerH.GetAllFactionMembers(guild).First(fetch => fetch.Username == toDemoteOffline.Username);
                    guild.DemoteMember(member);

                    ServerClient toDemoteOnline = ServerNetwork.Instance.GetConnectedClientFromUsername(toDemoteOffline.Username);
                    if (toDemoteOnline != null) toDemoteOnline.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
                }
            }
        }

        private static void SendMemberList(ServerClient client, PlayerGuildData factionManifest)
        {
            factionManifest._guild = GuildManagerH.GetFactionFromName(client.UserFile.GuildName);
            client.Listener.EnqueuePacket(PacketHeader.GuildManager, factionManifest);
        }
    }

    public static class GuildManagerH
    {
        public static GuildFile[] GetAllFactions()
        {
            List<GuildFile> factionFiles = new List<GuildFile>();

            foreach (string faction in Directory.GetFiles(Master.GuildsPath)) factionFiles.Add(Serializer.SerializeFromFile<GuildFile>(faction));

            return factionFiles.ToArray();
        }

        public static GuildFile GetFactionFromName(string name) { return GetAllFactions().FirstOrDefault(fetch => fetch.Name == name); }

        public static GuildMember[] GetAllFactionMembers(GuildFile file) { return file.GuildMembers.ToArray(); }

        public static bool CheckIfUserIsInFaction(GuildFile factionFile, string usernameToCheck)
        {
            return GetAllFactionMembers(factionFile).FirstOrDefault(fetch => fetch.Username == usernameToCheck) != null;
        }

        public static GuildRanks GetMemberRank(GuildFile factionFile, string usernameToCheck)
        {
            return GetAllFactionMembers(factionFile).First(fetch => fetch.Username == usernameToCheck).Rank;
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

        public static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            return GetAllFactions().FirstOrDefault(fetch => fetch.Name == nameToCheck) != null;
        }
    }
}
