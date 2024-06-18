using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class HediffOrder
    {
        public OnlineActivityPawnType pawnType;
        public OnlineActivityApplyMode applyMode;
        public int hediffTargetIndex;

        public string hediffDefName;
        public string hediffPartDefName;
        public float hediffSeverity;
        public bool hediffPermanent;
    }
}