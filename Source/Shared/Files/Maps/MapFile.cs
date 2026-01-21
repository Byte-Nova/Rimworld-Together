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

        public List<MapTile> Tiles { get; set; } = new List<MapTile>();

        public List<MapThing> Things { get; set; } = new List<MapThing>();

        public List<MapPawn> Pawns { get; set; } = new List<MapPawn>();
    }
}