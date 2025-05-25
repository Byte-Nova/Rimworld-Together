using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        [HandlesPacket(PacketHeader.MapManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            MapData data = Serializer.ConvertBytesToObject<MapData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            SaveUserMap(client, data._mapFile);
        }

        public static void SaveUserMap(ServerClient client, MapFile file)
        {
            file.UID = client.UserFile.Uid;
            Serializer.ObjectBytesToFile(Path.Combine(Master.MapsPath, file.Tile + fileExtension), file);

            InformationDisplayer.DisplaySaveMap(client);
        }

        public static void DeleteMap(MapFile mapFile)
        {
            File.Delete(Path.Combine(Master.MapsPath, mapFile.Tile + fileExtension));
            InformationDisplayer.DisplayRemoveMap(mapFile.Tile.ToString());
        }

        public static string[] GetAllMaps()
        {
            return Directory.GetFiles(Master.MapsPath);
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            if (toFind != null) return true;
            else return false;
        }

        public static MapFile GetMapFromTile(int mapTileToGet)
        {
            string path = Path.Combine(Master.MapsPath, mapTileToGet + fileExtension);
            return Serializer.FileBytesToObject<MapFile>(path);
        }
    }
}
