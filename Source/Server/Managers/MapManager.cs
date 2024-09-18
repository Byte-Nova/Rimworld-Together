using Shared;

namespace GameServer
{
    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapData mapData = Serializer.ConvertBytesToObject<MapData>(packet.contents);
            mapData._mapOwner = client.userFile.Username;

            byte[] compressedMapBytes = Serializer.ConvertObjectToBytes(mapData);
            File.WriteAllBytes(Path.Combine(Master.mapsPath, mapData._mapTile + fileExtension), compressedMapBytes);

            Logger.Message($"[Save map] > {client.userFile.Username} > {mapData._mapTile}");
        }

        public static void DeleteMap(MapData mapData)
        {
            if (mapData == null) return;

            File.Delete(Path.Combine(Master.mapsPath, mapData._mapTile + fileExtension));

            Logger.Warning($"[Remove map] > {mapData._mapTile}");
        }

        public static MapData[] GetAllMapFiles()
        {
            List<MapData> mapDatas = new List<MapData>();

            string[] maps = Directory.GetFiles(Master.mapsPath);
            foreach (string map in maps)
            {
                if (!map.EndsWith(fileExtension)) continue;
                byte[] decompressedBytes = File.ReadAllBytes(map);

                MapData newMap = Serializer.ConvertBytesToObject<MapData>(decompressedBytes);
                mapDatas.Add(newMap);
            }

            return mapDatas.ToArray();
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            MapData[] maps = GetAllMapFiles();
            foreach (MapData map in maps)
            {
                if (map._mapTile == mapTileToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static MapData[] GetAllMapsFromUsername(string username)
        {
            List<MapData> userMaps = new List<MapData>();

            SettlementFile[] userSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapData mapFile = GetUserMapFromTile(settlementFile.Tile);
                if (mapFile != null) userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapData GetUserMapFromTile(int mapTileToGet)
        {
            MapData[] mapFiles = GetAllMapFiles();

            foreach (MapData mapFile in mapFiles)
            {
                if (mapFile._mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }
    }
}
