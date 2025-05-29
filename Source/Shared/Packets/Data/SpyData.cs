using static Shared.CommonEnumerators;

namespace Shared
{

    public class SpyData
    {
        public SpyStepMode _stepMode { get; set; } = SpyStepMode.Request;

        public WorldObjectMode _worldObjectMode { get; set; } = WorldObjectMode.Settlement;

        public int _mapTile { get; set; } = -1;

        public MapFile _mapFile { get; set; } = null;
    }
}