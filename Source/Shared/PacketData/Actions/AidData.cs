using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class AidData
    {
        public AidStepMode stepMode;
        
        public int fromTile;

        public int toTile;

        public HumanData humanData;
    }
}