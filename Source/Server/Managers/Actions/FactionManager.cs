using GameServer.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class FactionManager
    {
        public enum FactionManifestMode
        {
            Create,
            Delete,
            NameInUse,
            NoPower,
            AddMember,
            RemoveMember,
            AcceptInvite,
            Promote,
            Demote,
            AdminProtection,
            MemberList
        }

        public enum FactionRanks { Member, Moderator, Admin }

        public static void ParseFactionPacket(Client client, Packet packet)
        {
            FactionManifestJSON factionManifest = Serializer.SerializeFromString<FactionManifestJSON>(packet.contents[0]);

            switch(int.Parse(factionManifest.manifestMode))
            {
                case (int)FactionManifestMode.Create:
                    CreateFaction(client, factionManifest);
                    break;

                case (int)FactionManifestMode.Delete:
                    DeleteFaction(client, factionManifest);
                    break;

                case (int)FactionManifestMode.AddMember:
                    AddMemberToFaction(client, factionManifest);
                    break;

                case (int)FactionManifestMode.RemoveMember:
                    RemoveMemberFromFaction(client, factionManifest);
                    break;

                case (int)FactionManifestMode.AcceptInvite:
                    ConfirmAddMemberToFaction(client, factionManifest);
                    break;

                case (int)FactionManifestMode.Promote:
                    PromoteMember(client, factionManifest);
                    break;

                case (int)FactionManifestMode.Demote:
                    DemoteMember(client, factionManifest);
                    break;

                case (int)FactionManifestMode.MemberList:
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

        public static FactionFile GetFactionFromClient(Client client)
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

        public static FactionRanks GetMemberRank(FactionFile factionFile, string usernameToCheck)
        {
            for(int i = 0; i < factionFile.factionMembers.Count(); i++)
            {
                if (factionFile.factionMembers[i] == usernameToCheck)
                {
                    return (FactionRanks)int.Parse(factionFile.factionMemberRanks[i]);
                }
            }

            return FactionRanks.Member;
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

        private static void CreateFaction(Client client, FactionManifestJSON factionManifest)
        {
            if (CheckIfFactionExistsByName(factionManifest.manifestDetails))
            {
                factionManifest.manifestMode = ((int)FactionManifestMode.NameInUse).ToString();

                string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                Packet packet = new Packet("FactionPacket", contents);
                Network.SendData(client, packet);
            }

            else
            {
                factionManifest.manifestMode = ((int)FactionManifestMode.Create).ToString();

                FactionFile factionFile = new FactionFile();
                factionFile.factionName = factionManifest.manifestDetails;
                factionFile.factionMembers.Add(client.username);
                factionFile.factionMemberRanks.Add(((int)FactionRanks.Admin).ToString());
                SaveFactionFile(factionFile);

                client.hasFaction = true;
                client.factionName = factionFile.factionName;

                UserFile userFile = UserManager.GetUserFile(client);
                userFile.hasFaction = true;
                userFile.factionName = factionFile.factionName;
                UserManager.SaveUserFile(client, userFile);

                string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                Packet packet = new Packet("FactionPacket", contents);
                Network.SendData(client, packet);

                Logger.WriteToConsole($"[Created faction] > {client.username} > {factionFile.factionName}", Logger.LogMode.Warning);
            }
        }

        private static void DeleteFaction(Client client, FactionManifestJSON factionManifest)
        {
            if (!CheckIfFactionExistsByName(client.factionName)) return;
            else
            {
                FactionFile factionFile = GetFactionFromClient(client);

                if (GetMemberRank(factionFile, client.username) != FactionRanks.Admin)
                {
                    ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
                }

                else
                {
                    factionManifest.manifestMode = ((int)FactionManifestMode.Delete).ToString();

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

                    string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                    Packet packet = new Packet("FactionPacket", contents);
                    foreach (string str in factionFile.factionMembers)
                    {
                        Client cClient = Network.connectedClients.ToList().Find(x => x.username == str);
                        if (cClient != null)
                        {
                            cClient.hasFaction = false;
                            cClient.factionName = "";
                            Network.SendData(cClient, packet);

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

        private static void AddMemberToFaction(Client client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
            Client toAdd = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            if (factionFile == null) return;
            else
            {
                if (GetMemberRank(factionFile, client.username) == FactionRanks.Member)
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
                                string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                                Packet packet = new Packet("FactionPacket", contents);
                                Network.SendData(toAdd, packet);
                            }
                        }
                    }
                }
            }
        }

        private static void ConfirmAddMemberToFaction(Client client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromFactionName(factionManifest.manifestDetails);

            if (factionFile == null) return;
            else
            {
                if (!factionFile.factionMembers.Contains(client.username))
                {
                    factionFile.factionMembers.Add(client.username);
                    factionFile.factionMemberRanks.Add(((int)FactionRanks.Member).ToString());
                    SaveFactionFile(factionFile);

                    client.hasFaction = true;
                    client.factionName = factionFile.factionName;

                    UserFile userFile = UserManager.GetUserFile(client);
                    userFile.hasFaction = true;
                    userFile.factionName = factionFile.factionName;
                    UserManager.SaveUserFile(client, userFile);

                    LikelihoodManager.ClearAllFactionMemberLikelihoods(factionFile);

                    Client[] members = GetAllConnectedFactionMembers(factionFile);
                    foreach (Client member in members) LikelihoodManager.UpdateClientLikelihoods(member);
                }
            }
        }

        private static void RemoveMemberFromFaction(Client client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
            UserFile toRemoveLocal = UserManager.GetUserFileFromName(settlementFile.owner);
            Client toRemove = UserManager.GetConnectedClientFromUsername(settlementFile.owner);

            if (GetMemberRank(factionFile, client.username) == FactionRanks.Member)
            {
                if (settlementFile.owner == client.username) RemoveFromFaction();
                else ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else if (GetMemberRank(factionFile, client.username) == FactionRanks.Moderator)
            {
                if (settlementFile.owner == client.username) RemoveFromFaction();
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) != FactionRanks.Member)
                        ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);

                    else RemoveFromFaction();
                }
            }

            else if (GetMemberRank(factionFile, client.username) == FactionRanks.Admin)
            {
                if (settlementFile.owner == client.username)
                {
                    factionManifest.manifestMode = ((int)FactionManifestMode.AdminProtection).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                    Packet packet = new Packet("FactionPacket", contents);
                    Network.SendData(client, packet);
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

                        string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
                        Packet packet = new Packet("FactionPacket", contents);
                        Network.SendData(toRemove, packet);

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

                    Client[] members = GetAllConnectedFactionMembers(factionFile);
                    foreach (Client member in members) LikelihoodManager.UpdateClientLikelihoods(member);
                }
            }
        }

        private static void PromoteMember(Client client, FactionManifestJSON factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) == FactionRanks.Member)
            {
                ResponseShortcutManager.SendNoPowerPacket(client, factionManifest);
            }

            else
            {
                if (!factionFile.factionMembers.Contains(userFile.username)) return;
                else
                {
                    if (GetMemberRank(factionFile, settlementFile.owner) == FactionRanks.Admin)
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

        private static void DemoteMember(Client client, FactionManifestJSON factionManifest)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(factionManifest.manifestDetails);
            UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);
            FactionFile factionFile = GetFactionFromClient(client);

            if (GetMemberRank(factionFile, client.username) != FactionRanks.Admin)
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

        private static Client[] GetAllConnectedFactionMembers(FactionFile factionFile)
        {
            List<Client> connectedFactionMembers = new List<Client>();
            foreach (Client client in Network.connectedClients.ToArray())
            {
                if (factionFile.factionMembers.Contains(client.username))
                {
                    connectedFactionMembers.Add(client);
                }
            }

            return connectedFactionMembers.ToArray();
        }

        private static void SendFactionMemberList(Client client, FactionManifestJSON factionManifest)
        {
            FactionFile factionFile = GetFactionFromClient(client);

            foreach(string str in factionFile.factionMembers)
            {
                factionManifest.manifestComplexDetails.Add(str);
                factionManifest.manifestSecondaryComplexDetails.Add(((int)GetMemberRank(factionFile, str)).ToString());
            }

            string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
            Packet packet = new Packet("FactionPacket", contents);
            Network.SendData(client, packet);
        }
    }
}
