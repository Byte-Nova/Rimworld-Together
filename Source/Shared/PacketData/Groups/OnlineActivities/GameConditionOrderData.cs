using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class GameConditionOrderData
    {
        public OnlineActivityApplyMode _applyMode;

        public string _conditionDefName;
        
        public int _duration;
    }
}