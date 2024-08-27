using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class RoadData
    {
        public RoadStepMode stepMode;
        
        public RoadDetails details;
    }
}