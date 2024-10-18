﻿using Shared;

namespace GameServer
{
    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            MapData data = Serializer.ConvertBytesToObject<MapData>(packet.contents);
            SaveUserMap(client, data);
        }

        public static void SaveUserMap(ServerClient client, MapData data)
        {
            data._mapFile.Owner = client.userFile.Username;

            Serializer.SerializeToFile(Path.Combine(Master.mapsPath, data._mapFile.Tile + fileExtension), data._mapFile);

            Logger.Message($"[Save map] > {client.userFile.Username} > {data._mapFile.Tile}");
        }

        public static void DeleteMap(MapFile mapFile)
        {
            File.Delete(Path.Combine(Master.mapsPath, mapFile.Tile + fileExtension));

            Logger.Warning($"[Remove map] > {mapFile.Tile}");
        }

        public static MapFile[] GetAllMapFiles()
        {
            List<MapFile> mapDatas = new List<MapFile>();

            string[] maps = Directory.GetFiles(Master.mapsPath);
            foreach (string map in maps)
            {
                if (!map.EndsWith(fileExtension)) continue;

                MapFile newMap = Serializer.SerializeFromFile<MapFile>(map);
                mapDatas.Add(newMap);
            }

            return mapDatas.ToArray();
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            MapFile[] maps = GetAllMapFiles();
            foreach (MapFile map in maps)
            {
                if (map.Tile == mapTileToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static MapFile[] GetAllMapsFromUsername(string username)
        {
            List<MapFile> userMaps = new List<MapFile>();

            SettlementFile[] userSettlements = PlayerSettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapFile mapFile = GetUserMapFromTile(settlementFile.Tile);
                if (mapFile != null) userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapFile GetUserMapFromTile(int mapTileToGet)
        {
            MapFile[] mapFiles = GetAllMapFiles();

            foreach (MapFile mapFile in mapFiles)
            {
                if (mapFile.Tile == mapTileToGet) return mapFile;
            }

            return null;
        }
    }
}
