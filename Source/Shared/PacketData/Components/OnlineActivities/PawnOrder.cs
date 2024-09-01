using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PawnOrder
    {
        public string defName;

        public int pawnIndex;

        public int targetCount;

        public int[] queueTargetCounts;

        public string[] targets;

        public int[] targetIndexes;

        public ActionTargetType[] targetTypes;

        public OnlineActivityTargetFaction[] targetFactions;

        public string[] queueTargetsA;

        public int[] queueTargetIndexesA;

        public ActionTargetType[] queueTargetTypesA;

        public OnlineActivityTargetFaction[] queueTargetFactionsA;

        public string[] queueTargetsB;

        public int[] queueTargetIndexesB;

        public ActionTargetType[] queueTargetTypesB;

        public OnlineActivityTargetFaction[] queueTargetFactionsB;

        public bool isDrafted;

        public int[] updatedPosition;
        
        public int updatedRotation;
    }
}