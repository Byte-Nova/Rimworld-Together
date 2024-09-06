using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class AidData
    {
        public AidStepMode _stepMode;
        
        public int _fromTile;

        public int _toTile;

        public HumanFile _humanData;
    }
}