using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineActivityData
    {
        public OfflineActivityStepMode _stepMode;

        public int _targetTile;
        
        public MapFile _mapFile;
    }
}