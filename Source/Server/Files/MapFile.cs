namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class MapFile
    {
        public string mapTile;

        public string mapOwner;

        public string mapSizeX;
        public string mapSizeY;
        public string mapSizeZ;

        public List<string> tileDefNames = new List<string>();

        public List<string> roofDefNames = new List<string>();

        public List<string> itemDetailsJSONS = new List<string>();

        public List<string> humanDetailsJSONS = new List<string>();

        public List<string> animalDetailsJSON = new List<string>();

        public List<string> mapMods = new List<string>();

        public string deflatedMapData;
    }
}
