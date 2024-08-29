﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class FactionManager
    {
        public static void ParseFactionPacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableFactions)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

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

        private static void CreateFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (FactionManagerHelper.CheckIfFactionExistsByName(factionManifest.factionFile.name))
            {
                factionManifest.stepMode = FactionStepMode.NameInUse;

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                factionManifest.stepMode = FactionStepMode.Create;

                FactionFile factionFile = new FactionFile();
                factionFile.name = factionManifest.factionFile.name;
                factionFile.currentMembers.Add(client.userFile.Username);
                factionFile.currentRanks.Add((int)FactionRanks.Admin);
                FactionManagerHelper.SaveFactionFile(factionFile);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                client.listener.EnqueuePacket(packet);

                Logger.Warning($"[Created faction] > {client.userFile.Username} > {factionFile.name}");
            }
        }

        private static void DeleteFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            if (!FactionManagerHelper.CheckIfFactionExistsByName(client.userFile.FactionFile.name)) return;
            else
            {
                FactionFile factionFile = client.userFile.FactionFile;

                if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest.stepMode = FactionStepMode.Delete;

                    UserFile[] toUpdateOffline = FactionManagerHelper.GetUsersFromFactionMembers(factionFile);
                    foreach (UserFile userFile in toUpdateOffline) userFile.UpdateFaction(null);

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                    foreach (ServerClient toUpdateConnected in FactionManagerHelper.GetConnectedFactionMembers(factionFile))
                    {
                        toUpdateConnected.userFile.UpdateFaction(null);
                        toUpdateConnected.listener.EnqueuePacket(packet);
                        GoodwillManager.UpdateClientGoodwills(toUpdateConnected);
                    }

                    SiteFile[] factionSites = FactionManagerHelper.GetFactionSites(factionFile);
                    foreach(SiteFile site in factionSites) SiteManager.DestroySiteFromFile(site);

                    File.Delete(Path.Combine(Master.factionsPath, factionFile.name + FactionManagerHelper.fileExtension));
                    Logger.Warning($"[Deleted Faction] > {client.userFile.Username} > {factionFile.name}");
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.FactionFile;
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.dataInt);
            ServerClient toAdd = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.Owner);

            if (factionFile == null) return;
            if (toAdd == null) return;

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            else
            {
                if (toAdd.userFile.FactionFile != null) return;
                else
                {
                    if (factionFile.currentMembers.Contains(toAdd.userFile.Username)) return;
                    else
                    {
                        factionManifest.factionFile.name = factionFile.name;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                        toAdd.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = FactionManagerHelper.GetFactionFromFactionName(factionManifest.factionFile.name);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.currentMembers.Contains(client.userFile.Username))
                {
                    factionFile.currentMembers.Add(client.userFile.Username);
                    factionFile.currentRanks.Add((int)FactionRanks.Member);
                    FactionManagerHelper.SaveFactionFile(factionFile);

                    GoodwillManager.ClearAllFactionMemberGoodwills(factionFile);

                    ServerClient[] members = FactionManagerHelper.GetConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) GoodwillManager.UpdateClientGoodwills(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, PlayerFactionData factionManifest)
        {
            FactionFile factionFile = client.userFile.FactionFile;
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.dataInt);
            UserFile toUpdateOffline = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);
            ServerClient toRemoveConnected = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.Owner);

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
                    factionManifest.stepMode = FactionStepMode.AdminProtection;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
                    client.listener.EnqueuePacket(packet);
                }
                else RemoveFromFaction();
            }

            void RemoveFromFaction()
            {
                if (!factionFile.currentMembers.Contains(settlementFile.Owner)) return;
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
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.dataInt);
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
                if (!factionFile.currentMembers.Contains(userFile.Username)) return;
                else
                {
                    for (int i = 0; i < factionFile.currentMembers.Count(); i++)
                    {
                        if (factionFile.currentMembers[i] == userFile.Username)
                        {
                            factionFile.currentRanks[i] = 1;
                            FactionManagerHelper.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void DemoteMember(ServerClient client, PlayerFactionData factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.dataInt);
            UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);
            FactionFile factionFile = client.userFile.FactionFile;

            if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Admin)
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
                            FactionManagerHelper.SaveFactionFile(factionFile);
                            break;
                        }
                    }
                }
            }
        }

        private static void SendFactionMemberList(ServerClient client, PlayerFactionData factionManifest)
        {
            factionManifest.factionFile = client.userFile.FactionFile;
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), factionManifest);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class FactionManagerHelper
    {
        //Variables

        public readonly static string fileExtension = ".mpfaction";

        public static void SaveFactionFile(FactionFile factionFile)
        {
            factionFile.savingSemaphore.WaitOne();

            try
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

                SiteFile[] factionSites = GetFactionSites(factionFile);
                foreach(SiteFile site in factionSites) SiteManagerHelper.UpdateFaction(site, factionFile);
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            factionFile.savingSemaphore.Release();
        }

        public static bool CheckIfFactionExistsByName(string nameToCheck)
        {
            FactionFile factionFile = GetAllFactions().FirstOrDefault(fetch => fetch.name == nameToCheck);
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
                if (factionFile.name == factionName) return factionFile;
            }

            return null;
        }

        public static bool CheckIfUserIsInFaction(FactionFile factionFile, string usernameToCheck)
        {
            if (factionFile.currentMembers.Contains(usernameToCheck)) return true;
            else return false;
        }

        public static FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for (int i = 0; i < factionFile.currentMembers.Count(); i++)
            {
                if (factionFile.currentMembers[i] == usernameToCheck)
                {
                    return (FactionRanks)factionFile.currentRanks[i];
                }
            }

            return FactionRanks.Member;
        }

        public static SiteFile[] GetFactionSites(FactionFile factionFile)
        {
            return SiteManagerHelper.GetAllSites().Where(fetch => fetch.FactionFile != null && 
                fetch.FactionFile.name == factionFile.name).ToArray();
        }

        public static ServerClient[] GetConnectedFactionMembers(FactionFile factionFile)
        {
            return NetworkHelper.GetConnectedClientsSafe().Where(fetch => fetch.userFile.FactionFile != null && 
                fetch.userFile.FactionFile.name == factionFile.name).ToArray();
        }

        public static UserFile[] GetUsersFromFactionMembers(FactionFile factionFile)
        {
            return UserManagerHelper.GetAllUserFiles().Where(fetch => fetch.FactionFile != null && 
                fetch.FactionFile.name == factionFile.name).ToArray();
        }
    }
}
