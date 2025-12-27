using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using Shared.Files.Maps;

namespace GameServer.Managers
{
    public static class MapManager
    {
        [HandlesPacket(PacketHeader.MapManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            MapData data = Serializer.ConvertBytesToObject<MapData>(bytes);

            SaveUserMap(client, data);
        }

        public static void SaveUserMap(ServerClient client, MapData data)
        {
            File.WriteAllBytes(Path.Combine(Master.MapsPath, data._mapTile + CommonValues.DefaultSaveFormat), data._rawData);
            InformationDisplayer.DisplaySaveMap(client);
        }

        public static string[] GetAllMaps() { return Directory.GetFiles(Master.MapsPath); }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            if (toFind != null) return true;
            else return false;
        }

        public static byte[] GetMapFromTile(int mapTileToGet)
        {
            string path = Path.Combine(Master.MapsPath, mapTileToGet + CommonValues.DefaultSaveFormat);
            if (File.Exists(path)) return File.ReadAllBytes(path);
            else return null;
        }
    }
}
