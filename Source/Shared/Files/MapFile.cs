namespace Shared
{
    public class MapFile
    {
        public int Tile;

        public int[] Size;

        public string UID;

        public string CurWeatherDefName;

        public ModConfigFile Mods;

        public MapTileDetails[] Tiles = new MapTileDetails[0];

        public ThingFile[] FactionThings;

        public ThingFile[] NonFactionThings;

        public HumanFile[] FactionHumans;

        public HumanFile[] NonFactionHumans;

        public AnimalFile[] FactionAnimals;
        
        public AnimalFile[] NonFactionAnimals;
    }
}