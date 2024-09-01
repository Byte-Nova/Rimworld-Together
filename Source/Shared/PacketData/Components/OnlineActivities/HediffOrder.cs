using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class HediffOrder
    {
        public OnlineActivityTargetFaction pawnFaction;

        public OnlineActivityApplyMode applyMode;

        public int hediffTargetIndex;

        public string hediffDefName;

        public string hediffPartDefName;

        public string hediffWeaponDefName;

        public float hediffSeverity;
        
        public bool hediffPermanent;
    }
}