using Shared.Files;

namespace TCPNetwork.Packets
{
    public class MapData
    {
        public MapFile _mapFile { get; set; } = new MapFile();

        public override string ToString()
        {
            return $"MapData:|{_mapFile}";
        }
    }
}