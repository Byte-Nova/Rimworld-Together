using Shared.Files.Maps;

namespace TCPNetwork.Packets
{
    public class MapData
    {
        public int _mapTile { get; set; } = -1;

        public byte[] _rawData { get; set; } = null;

        public MapFile _mapFile { get; set; } = new MapFile();
    }
}