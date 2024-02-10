using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class FactionManager
    {
        public static void ParseFactionPacket(ServerClient client, Packet packet)
        {
            FactionManifestJSON factionManifest = (FactionManifestJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

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

            string[] factions = Directory.GetFiles(Program.factionsPath);
            foreach(string faction in factions)
            {
                factionFiles.Add(Serializer.SerializeFromFile<FactionFile>(faction));
            }

            return factionFiles.ToArray();
        }

        public static FactionFile GetFactionFromClient(ServerClient client)
        {
            string[] factions = Directory.GetFiles(Program.factionsPath);
            foreach (string faction in factions)
            {
                FactionFile factionFile = Serializer.SerializeFromFile<FactionFile>(faction);
                if (factionFile.factionName == client.factionName) return factionFile;
            }

            return null;
        }

        public static FactionFile GetFactionFromFactionName(string factionName)
        {
            string[] factions = Directory.GetFiles(Program.factionsPath);
            foreach (string faction in factions)
            {
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
            string savePath = Path.Combine(Program.factionsPath, factionFile.factionName + ".json");
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

        private static void CreateFaction(ServerClient client, FactionManifestJSON factionManifest)
        {
            if (CheckIfFactionExistsByName(factionManifest.manifestDetails))
            {
                factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NameInUse).ToString();

                Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                client.clientListener.SendData(packet);
            }

            else
            {
                factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.Create).ToString();

                FactionFile factionFile = new FactionFile();
                factionFile.factionName = factionManifest.manifestDetails;
                factionFile.factionMembers.Add(client.username);
                factionFile.factionMemberRanks.Add(((int)CommonEnumerators.FactionRanks.Admin).ToString());
                SaveFactionFile(factionFile);

                client.hasFaction = true;
                client.factionName = factionFile.factionName;

                UserFile userFile = UserManager.GetUserFile(client);
                userFile.hasFaction = true;
                userFile.factionName = factionFile.factionName;
                UserManager.SaveUserFile(client, userFile);

                Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                client.clientListener.SendData(packet);

                Logger.WriteToConsole($"[Created faction] > {client.username} > {factionFile.factionName}", Logger.LogMode.Warning);
            }
        }

        private static void DeleteFaction(ServerClient client, FactionManifestJSON factionManifest)
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

                    Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                    foreach (string str in factionFile.factionMembers)
                    {
                        ServerClient cClient = Network.Network.connectedClients.ToList().Find(x => x.username == str);
                        if (cClient != null)
                        {
                            cClient.hasFaction = false;
                            cClient.factionName = "";
                            cClient.clientListener.SendData(packet);

                            LikelihoodManager.UpdateClientLikelihoods(cClient);
                        }
                    }

                    SiteFile[] factionSites = GetFactionSites(factionFile);
                    foreach(SiteFile site in factionSites) SiteManager.DestroySiteFromFile(site);

                    File.Delete(Path.Combine(Program.factionsPath, factionFile.factionName + ".json"));
                    Logger.WriteToConsole($"[Deleted Faction] > {client.username} > {factionFile.factionName}", Logger.LogMode.Warning);
                }
            }
        }

        private static void AddMemberToFaction(ServerClient client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
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
                                factionManifest.manifestDetails = factionFile.factionName;
                                Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                                toAdd.clientListener.SendData(packet);
                            }
                        }
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(ServerClient client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromFactionName(factionManifest.manifestDetails);

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

                    LikelihoodManager.ClearAllFactionMemberLikelihoods(factionFile);

                    ServerClient[] members = GetAllConnectedFactionMembers(factionFile);
                    foreach (ServerClient member in members) LikelihoodManager.UpdateClientLikelihoods(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(ServerClient client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
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
                    Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                    client.clientListener.SendData(packet);
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

                        Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
                        toRemove.clientListener.SendData(packet);

                        LikelihoodManager.UpdateClientLikelihoods(toRemove);
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
                    foreach (ServerClient member in members) LikelihoodManager.UpdateClientLikelihoods(member);
                }
            }
        }

        private static void PromoteMember(ServerClient client, FactionManifestJSON factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
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

        private static void DemoteMember(ServerClient client, FactionManifestJSON factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
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
            foreach (ServerClient client in Network.Network.connectedClients.ToArray())
            {
                if (factionFile.factionMembers.Contains(client.username))
                {
                    connectedFactionMembers.Add(client);
                }
            }

            return connectedFactionMembers.ToArray();
        }

        private static void SendFactionMemberList(ServerClient client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);

            foreach(string str in factionFile.factionMembers)
            {
                factionManifest.manifestComplexDetails.Add(str);
                factionManifest.manifestSecondaryComplexDetails.Add(((int)GetMemberRank(factionFile, str)).ToString());
            }

            Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
            client.clientListener.SendData(packet);
        }
    }
}
