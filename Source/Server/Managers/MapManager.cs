﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class MapManager
    {
        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapFileJSON mapFileJSON = (MapFileJSON)Serializer.ConvertBytesToObject(packet.contents);
            mapFileJSON.mapOwner = client.username;

            byte[] compressedMapBytes = GZip.Compress(Serializer.ConvertObjectToBytes(mapFileJSON));
            File.WriteAllBytes(Path.Combine(Master.mapsPath, mapFileJSON.mapTile + ".mpmap"), compressedMapBytes);

            Logger.WriteToConsole($"[Save map] > {client.username} > {mapFileJSON.mapTile}");
        }

        public static void DeleteMap(MapFileJSON mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Master.mapsPath, mapFile.mapTile + ".json"));

            Logger.WriteToConsole($"[Remove map] > {mapFile.mapTile}", LogMode.Warning);
        }

        public static MapFileJSON[] GetAllMapFiles()
        {
            List<MapFileJSON> mapDetails = new List<MapFileJSON>();

            string[] maps = Directory.GetFiles(Master.mapsPath);
            foreach (string str in maps)
            {
                byte[] decompressedBytes = GZip.Decompress(File.ReadAllBytes(str));

                MapFileJSON newMap = (MapFileJSON)Serializer.ConvertBytesToObject(decompressedBytes);
                mapDetails.Add(newMap);
            }

            return mapDetails.ToArray();
        }

        public static bool CheckIfMapExists(string mapTileToCheck)
        {
            MapFileJSON[] maps = GetAllMapFiles();
            foreach (MapFileJSON map in maps)
            {
                if (map.mapTile == mapTileToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static MapFileJSON[] GetAllMapsFromUsername(string username)
        {
            List<MapFileJSON> userMaps = new List<MapFileJSON>();

            SettlementFile[] userSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapFileJSON mapFile = GetUserMapFromTile(settlementFile.tile);
                userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapFileJSON GetUserMapFromTile(string mapTileToGet)
        {
            MapFileJSON[] mapFiles = GetAllMapFiles();

            foreach (MapFileJSON mapFile in mapFiles)
            {
                if (mapFile.mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }
    }
}
