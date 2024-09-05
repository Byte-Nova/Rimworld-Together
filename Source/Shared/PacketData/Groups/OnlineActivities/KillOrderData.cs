using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class KillOrderData
    {
        public OnlineActivityTargetFaction _pawnFaction;
        
        public int _killTargetIndex;
    }
}