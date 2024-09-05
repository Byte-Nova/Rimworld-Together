using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PawnOrderData
    {
        public string _defName;

        public int _pawnIndex;

        public int _targetCount;

        public int[] _queueTargetCounts;

        public string[] _targets;

        public int[] _targetIndexes;

        public ActionTargetType[] _targetTypes;

        public OnlineActivityTargetFaction[] _targetFactions;

        public string[] _queueTargetsA;

        public int[] _queueTargetIndexesA;

        public ActionTargetType[] _queueTargetTypesA;

        public OnlineActivityTargetFaction[] _queueTargetFactionsA;

        public string[] _queueTargetsB;

        public int[] _queueTargetIndexesB;

        public ActionTargetType[] _queueTargetTypesB;

        public OnlineActivityTargetFaction[] _queueTargetFactionsB;

        public bool _isDrafted;

        public int[] _updatedPosition;
        
        public int _updatedRotation;
    }
}