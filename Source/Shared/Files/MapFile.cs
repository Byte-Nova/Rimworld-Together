namespace Shared
{
    public class MapFile
    {
        public int Tile;

        public int[] Size;

        public string Owner;

        public string[] Mods;

        public string CurWeatherDefName;

        public TileComponent[] Tiles = new TileComponent[0];

        public ThingFile[] FactionThings;

        public ThingFile[] NonFactionThings;

        public HumanFile[] FactionHumans;

        public HumanFile[] NonFactionHumans;

        public AnimalFile[] FactionAnimals;
        
        public AnimalFile[] NonFactionAnimals;
    }
}