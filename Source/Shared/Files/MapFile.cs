namespace Shared.Files
{
    public class MapFile
    {
        public int Tile { get; set; } = -1;

        public int[] Size { get; set; } = null;

        public string UID { get; set; } = string.Empty;

        public int Wealth { get; set; } = -1;

        public string CurWeatherDefName { get; set; } = string.Empty;

        public ModConfigFile Mods { get; set; } = null;

        public MapTileDetails[] Tiles { get; set; } = new MapTileDetails[0];

        public string[] FactionThings { get; set; } = null;

        public string[] NonFactionThings { get; set; } = null;

        public HumanFile[] FactionHumans { get; set; } = null;

        public HumanFile[] NonFactionHumans { get; set; } = null;

        public string[] FactionAnimals { get; set; } = null;

        public string[] NonFactionAnimals { get; set; } = null;
    }
}