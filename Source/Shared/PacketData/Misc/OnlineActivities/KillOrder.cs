using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class KillOrder
    {
        public OnlineActivityTargetFaction pawnFaction;
        
        public int killTargetIndex;
    }
}