using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PawnOrder
    {
        public int pawnIndex;
        public string defName;
        public string actionTargetA;
        public int actionTargetIndex;
        public ActionTargetType actionTargetType;

        public bool isDrafted;
        public string positionSync;
        public int rotationSync;
    }
}