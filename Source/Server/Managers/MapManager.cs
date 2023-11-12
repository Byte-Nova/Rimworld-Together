using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers
{
    public static class MapManager
    {
        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            mapDetailsJSON.mapOwner = client.username;

            byte[] compressedMapBytes = ObjectConverter.ConvertObjectToBytes(mapDetailsJSON);
            //byte[] compressedMapBytes = GZip.Compress(ObjectConverter.ConvertObjectToBytes(mapDetailsJSON));
            File.WriteAllBytes(Path.Combine(Program.mapsPath, mapDetailsJSON.mapTile + ".json"), compressedMapBytes);

            Logger.WriteToConsole($"[Save map] > {client.username} > {mapDetailsJSON.mapTile}");
        }

        public static void DeleteMap(MapDetailsJSON mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Program.mapsPath, mapFile.mapTile + ".json"));

            Logger.WriteToConsole($"[Remove map] > {mapFile.mapTile}", Logger.LogMode.Warning);
        }

        public static MapDetailsJSON[] GetAllMapFiles()
        {
            List<MapDetailsJSON> mapDetails = new List<MapDetailsJSON>();

            string[] maps = Directory.GetFiles(Program.mapsPath);
            foreach (string str in maps)
            {
                byte[] fileBytes = File.ReadAllBytes(str);
                //byte[] decompressedBytes = GZip.Decompress(fileBytes);

                MapDetailsJSON newMap = (MapDetailsJSON)ObjectConverter.ConvertBytesToObject(fileBytes);
                mapDetails.Add(newMap);
            }

            return mapDetails.ToArray();
        }

        public static bool CheckIfMapExists(string mapTileToCheck)
        {
            MapDetailsJSON[] maps = GetAllMapFiles();
            foreach (MapDetailsJSON map in maps)
            {
                if (map.mapTile == mapTileToCheck)
                {
                    return true;
                }
            }

            return false;
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

            foreach (MapDetailsJSON mapFile in mapFiles)
            {
                if (mapFile.mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }
    }
}
