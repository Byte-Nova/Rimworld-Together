using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PawnOrder
    {
        public string defName;
        public int pawnIndex;
        public int count;

        public string[] actionTargets;
        public int[] actionIndexes;
        public ActionTargetType[] actionTypes;

        public string[] queueTargetsA;
        public int[] queueIndexesA;
        public ActionTargetType[] queueTypesA;

        public string[] queueTargetsB;
        public int[] queueIndexesB;
        public ActionTargetType[] queueTypesB;

        public bool isDrafted;
        public string positionSync;
        public int rotationSync;
    }
}