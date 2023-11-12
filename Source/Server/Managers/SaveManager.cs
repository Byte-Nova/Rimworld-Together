using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;
using System.Security.Cryptography;


namespace RimworldTogether.GameServer.Managers
{
    public static class SaveManager
    {
        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach(string save in saves) if (Path.GetFileNameWithoutExtension(save) == client.username) return true;
            return false;
        }

        public static bool CheckIfMapExists(string mapTileToCheck)
        {
            string[] maps = Directory.GetFiles(Program.mapsPath);
            foreach(string str in maps)
            {
                MapDetailsJSON mapDetailsJSON = Serializer.SerializeFromFile<MapDetailsJSON>(str);
                if (mapDetailsJSON.mapTile == mapTileToCheck) return true;
            }

            return false;
        }

        public static MapDetailsJSON[] GetAllMapFiles()
        {
            List<MapDetailsJSON> mapDetails = new List<MapDetailsJSON>();
            string[] maps = Directory.GetFiles(Program.mapsPath);
            foreach (string str in maps) mapDetails.Add(Serializer.SerializeFromFile<MapDetailsJSON>(str));
            return mapDetails.ToArray();
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach (string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == username)
                {
                    return File.ReadAllBytes(save);
                }
            }

            return null;
        }

        public static void SaveUserGame(ServerClient client, Packet packet)
        {
            SaveFileJSON saveFileJSON = (SaveFileJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            byte[] compressedSave = GZip.Compress(saveFileJSON.saveData);

            File.WriteAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave"), compressedSave);

            if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Disconnect).ToString())
            {
                CommandManager.SendDisconnectCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To menu");
            }

            else if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Quit).ToString())
            {
                CommandManager.SendQuitCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > Quiting");
            }

            else if (saveFileJSON.saveMode == ((int)CommonEnumerators.SaveMode.Transfer).ToString())
            {
                Logger.WriteToConsole($"[Save game] > {client.username} > Item transfer");
            }

            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
        }

        public static void LoadUserGame(ServerClient client)
        {
            byte[] decompressedSave = GZip.Decompress(File.ReadAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave")));
            SaveFileJSON saveFileJSON = new SaveFileJSON();
            saveFileJSON.saveData = decompressedSave;

            Packet packet = Packet.CreatePacketFromJSON("LoadFilePacket", saveFileJSON);
            client.clientListener.SendData(packet);

            Logger.WriteToConsole($"[Load game] > {client.username}");
        }

        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            mapDetailsJSON.mapOwner = client.username;

            Serializer.SerializeToFile(Path.Combine(Program.mapsPath, mapDetailsJSON.mapTile + ".json"), mapDetailsJSON);
            Logger.WriteToConsole($"[Save map] > {client.username} > {mapDetailsJSON.mapTile}");
        }

        public static void DeleteMap(MapDetailsJSON mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Program.mapsPath, mapFile.mapTile + ".json"));

            Logger.WriteToConsole($"[Remove map] > {mapFile.mapTile}", Logger.LogMode.Warning);
        }

        public static MapDetailsJSON[] GetAllMapsFromUsername(string username)
        {
            List<MapDetailsJSON> userMaps = new List<MapDetailsJSON>();

            SettlementFile[] userSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapDetailsJSON mapFile = GetUserMapFromTile(settlementFile.tile);
                userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapDetailsJSON GetUserMapFromTile(string mapTileToGet)
        {
            MapDetailsJSON[] mapFiles = GetAllMapFiles();

            foreach(MapDetailsJSON mapFile in mapFiles)
            {
                if (mapFile.mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }

        public static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                client.disconnectFlag = true;

                string[] saves = Directory.GetFiles(Program.savesPath);

                string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == client.username);
                if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

                Logger.WriteToConsole($"[Delete save] > {client.username}", Logger.LogMode.Warning);

                MapDetailsJSON[] userMaps = GetAllMapsFromUsername(client.username);
                foreach (MapDetailsJSON map in userMaps) DeleteMap(map);

                SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.username);
                foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

                SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
                foreach (SettlementFile settlementFile in playerSettlements)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlementFile.tile;
                    settlementDetailsJSON.owner = settlementFile.owner;

                    SettlementManager.RemoveSettlement(client, settlementDetailsJSON);
                }
            }
        }

        public static void DeletePlayerDetails(string username)
        {
            ServerClient connectedUser = UserManager.GetConnectedClientFromUsername(username);
            if (connectedUser != null) connectedUser.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Program.savesPath);
            string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == username);
            if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

            MapDetailsJSON[] userMaps = GetAllMapsFromUsername(username);
            foreach (MapDetailsJSON map in userMaps) DeleteMap(map);

            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                settlementDetailsJSON.tile = settlementFile.tile;
                settlementDetailsJSON.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(null, settlementDetailsJSON, false);
            }

            Logger.WriteToConsole($"[Deleted player details] > {username}", Logger.LogMode.Warning);
        }
    }
}
