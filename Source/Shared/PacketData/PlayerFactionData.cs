using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerFactionData
    {
        public FactionStepMode stepMode;

        public string manifestDataString;

        public int manifestDataInt;

        public List<string> manifestComplexData = new List<string>();

        public List<string> manifestSecondaryComplexData = new List<string>();
    }
}
