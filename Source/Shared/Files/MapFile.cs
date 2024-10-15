namespace Shared
{
    public class MapFile
    {
        public int Tile;

        public int[] Size;

        public string Owner;

        public string CurWeatherDefName;

        public ModConfigFile Mods;

        public TileComponent[] Tiles = new TileComponent[0];

        public ThingDataFile[] FactionThings;

        public ThingDataFile[] NonFactionThings;

        public HumanFile[] FactionHumans;

        public HumanFile[] NonFactionHumans;

        public AnimalFile[] FactionAnimals;
        
        public AnimalFile[] NonFactionAnimals;
    }
}