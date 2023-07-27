using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers
{
    public static class SaveManager
    {
        public enum SaveMode { Disconnect, Quit, Autosave, Transfer, Event }

        public enum MapMode { Save, Load }

        public static bool CheckIfUserHasSave(Client client)
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
                MapFile mapFile = Serializer.SerializeFromFile<MapFile>(str);
                if (mapFile.mapTile == mapTileToCheck) return true;
            }

            return false;
        }

        public static MapFile[] GetAllMapFiles()
        {
            List<MapFile> mapFiles = new List<MapFile>();
            string[] maps = Directory.GetFiles(Program.mapsPath);
            foreach (string str in maps) mapFiles.Add(Serializer.SerializeFromFile<MapFile>(str));
            return mapFiles.ToArray();
        }

        public static SaveFileJSON GetUserSave(Client client)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach (string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == client.username)
                {
                    return Serializer.SerializeFromFile<SaveFileJSON>(save);
                }
            }

            return null;
        }

        public static void SaveUserGame(Client client, Packet packet)
        {
            SaveFileJSON saveFileJSON = Serializer.SerializeFromString<SaveFileJSON>(packet.contents[0]);
            File.WriteAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave"), Convert.FromBase64String(saveFileJSON.saveData));

            if (saveFileJSON.saveMode == ((int)SaveMode.Disconnect).ToString())
            {
                CommandManager.SendDisconnectCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To menu");
            }

            else if (saveFileJSON.saveMode == ((int)SaveMode.Quit).ToString())
            {
                CommandManager.SendQuitCommand(client);

                client.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > Quiting");
            }

            else if (saveFileJSON.saveMode == ((int)SaveMode.Transfer).ToString())
            {
                Logger.WriteToConsole($"[Save game] > {client.username} > Item transfer");
            }

            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
        }

        public static void LoadUserGame(Client client)
        {
            string[] contents = new string[] { Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Program.savesPath, client.username + ".mpsave"))) };
            Packet packet = new Packet("LoadFilePacket", contents);
            Network.Network.SendData(client, packet);

            Logger.WriteToConsole($"[Load game] > {client.username} {contents.GetHashCode()}");
        }

        public static void SaveUserMap(Client client, Packet packet)
        {
            MapDetailsJSON mapDetailsJSON = Serializer.SerializeFromString<MapDetailsJSON>(packet.contents[0]);

            MapFile mapFile = new MapFile();
            mapFile.mapTile = mapDetailsJSON.mapTile;
            mapFile.mapOwner = client.username;
            mapFile.deflatedMapData = mapDetailsJSON.deflatedMapData;

            Serializer.SerializeToFile(Path.Combine(Program.mapsPath, mapFile.mapTile + ".json"), mapFile);
            Logger.WriteToConsole($"[Save map] > {client.username} > {mapFile.mapTile}");
        }

        public static void DeleteMap(MapFile mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Program.mapsPath, mapFile.mapTile + ".json"));

            Logger.WriteToConsole($"[Delete map] > {mapFile.mapTile}", Logger.LogMode.Warning);
        }

        public static MapFile[] GetAllUserMaps(Client client)
        {
            List<MapFile> userMaps = new List<MapFile>();

            SettlementFile[] userSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapFile mapFile = GetUserMapFromTile(settlementFile.tile);
                userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapFile GetUserMapFromTile(string mapTileToGet)
        {
            MapFile[] mapFiles = GetAllMapFiles();

            foreach(MapFile mapFile in mapFiles)
            {
                if (mapFile.mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }

        public static void ResetClientSave(Client client)
        {
            if (!CheckIfUserHasSave(client)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                client.disconnectFlag = true;

                string[] saves = Directory.GetFiles(Program.savesPath);
                string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == client.username);
                File.Delete(toDelete);

                Logger.WriteToConsole($"[Delete save] > {client.username}", Logger.LogMode.Warning);

                MapFile[] userMaps = GetAllUserMaps(client);
                foreach (MapFile map in userMaps) DeleteMap(map);

                SiteFile[] playerSites = SiteManager.GetAllSitesFromUser(client);
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
    }
}
