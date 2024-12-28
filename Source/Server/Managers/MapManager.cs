using Shared;

namespace GameServer
{
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
            string savingDirectory = Path.Combine(Master.mapsPath, client.userFile.Username);
            if (!Directory.Exists(savingDirectory)) Directory.CreateDirectory(savingDirectory);

            file.Owner = client.userFile.Username;
            Serializer.ObjectBytesToFile(Path.Combine(savingDirectory, file.Tile + fileExtension), file);

            Logger.Message($"[Save map] > {client.userFile.Username} > {file.Tile}");
        }

        public static void DeleteMap(MapFile mapFile)
        {
            string filePath = Path.Combine(Master.mapsPath, mapFile.Owner, mapFile.Tile + fileExtension);

            File.Delete(filePath);
            Logger.Warning($"[Remove map] > {Path.GetFileNameWithoutExtension(filePath)}");
        }

        public static string[] GetAllMaps()
        {
            return Directory.GetFiles(Master.mapsPath, "*.mpmap", SearchOption.AllDirectories);
        }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            if (toFind != null) return true;
            else return false;
        }

        public static MapFile[] GetAllMapsFromUsername(string username)
        {
            List<MapFile> allUserMaps = new List<MapFile>();
            string[] allMapPaths = Directory.GetFiles(Path.Combine(Master.mapsPath, username));
            foreach (string str in allMapPaths) allUserMaps.Add(Serializer.FileBytesToObject<MapFile>(str));

            return allUserMaps.ToArray();
        }

        public static MapFile GetUserMapFromTile(string username, int mapTileToGet)
        {
            string path = Path.Combine(Master.mapsPath, username, mapTileToGet + fileExtension);
            return Serializer.FileBytesToObject<MapFile>(path);
        }
    }
}
