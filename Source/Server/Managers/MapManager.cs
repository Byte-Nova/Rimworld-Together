using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers
{
    public static class MapManager
    {
        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapFileJSON mapFileJSON = (MapFileJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            mapFileJSON.mapOwner = client.username;

            byte[] compressedMapBytes = GZip.Compress(ObjectConverter.ConvertObjectToBytes(mapFileJSON));
            File.WriteAllBytes(Path.Combine(Program.mapsPath, mapFileJSON.mapTile + ".mpmap"), compressedMapBytes);

            Logger.WriteToConsole($"[Save map] > {client.username} > {mapFileJSON.mapTile}");
        }

        public static void DeleteMap(MapFileJSON mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Program.mapsPath, mapFile.mapTile + ".json"));

            Logger.WriteToConsole($"[Remove map] > {mapFile.mapTile}", Logger.LogMode.Warning);
        }

        public static MapFileJSON[] GetAllMapFiles()
        {
            List<MapFileJSON> mapDetails = new List<MapFileJSON>();

            string[] maps = Directory.GetFiles(Program.mapsPath);
            foreach (string str in maps)
            {
                byte[] decompressedBytes = GZip.Decompress(File.ReadAllBytes(str));

                MapFileJSON newMap = (MapFileJSON)ObjectConverter.ConvertBytesToObject(decompressedBytes);
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
