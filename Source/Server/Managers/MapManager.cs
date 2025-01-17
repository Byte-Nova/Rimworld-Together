using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;

namespace GameServer.Managers
{
    [RTManager]
    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            MapData mapData = Serializer.ConvertBytesToObject<MapData>(packet.contents);
            SaveUserMap(client, mapData._mapFile);
        }

        public static void SaveUserMap(ServerClient client, MapFile file)
        {
            file.UID = client.userFile.Uid;
            Serializer.ObjectBytesToFile(Path.Combine(Master.mapsPath, file.Tile + fileExtension), file);

            InformationDisplayer.DisplaySaveMap(client);
        }

        public static void DeleteMap(MapFile mapFile)
        {
            File.Delete(Path.Combine(Master.mapsPath, mapFile.Tile + fileExtension));
            InformationDisplayer.DisplayRemoveMap(mapFile.Tile.ToString());
        }

        public static string[] GetAllMaps()
        {
            return Directory.GetFiles(Master.mapsPath);
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            if (toFind != null) return true;
            else return false;
        }

        public static MapFile GetUserMapFromTile(int mapTileToGet)
        {
            string path = Path.Combine(Master.mapsPath, mapTileToGet + fileExtension);
            return Serializer.FileBytesToObject<MapFile>(path);
        }
    }
}
