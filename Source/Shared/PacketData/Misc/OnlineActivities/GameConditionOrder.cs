using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class GameConditionOrder
    {
        public OnlineActivityApplyMode applyMode;

        public string conditionDefName;
        
        public int duration;
    }
}