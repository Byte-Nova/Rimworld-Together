using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class HediffOrderData
    {
        public OnlineActivityTargetFaction _pawnFaction;

        public OnlineActivityApplyMode _applyMode;

        public int _hediffTargetIndex;

        public string _hediffDefName;

        public string _hediffPartDefName;

        public string _hediffWeaponDefName;

        public float _hediffSeverity;
        
        public bool _hediffPermanent;
    }
}