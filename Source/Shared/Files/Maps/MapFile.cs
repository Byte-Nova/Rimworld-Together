using Shared.Files.Configs.Mods;
using System.Collections.Generic;

namespace Shared.Files.Maps
{
    public class MapFile
    {
        public int Tile { get; set; } = -1;

        public int[] Size { get; set; } = null;

        public int Wealth { get; set; } = -1;

        public byte WeatherByte { get; set; } = byte.MaxValue;

        public ModsConfigFile Mods { get; set; } = new ModsConfigFile();

        public List<MapTile> Tiles { get; set; } = new List<MapTile>();

        public List<string> FactionThings { get; set; } = new List<string>();

        public List<string> NonFactionThings { get; set; } = new List<string>();

        public List<HumanFile> FactionHumans { get; set; } = new List<HumanFile>();

        public List<HumanFile> NonFactionHumans { get; set; } = new List<HumanFile>();

        public List<string> FactionAnimals { get; set; } = new List<string>();

        public List<string> NonFactionAnimals { get; set; } = new List<string>();
    }
}