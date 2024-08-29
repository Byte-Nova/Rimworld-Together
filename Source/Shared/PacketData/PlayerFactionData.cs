using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerFactionData
    {
        public FactionStepMode stepMode;

        public FactionFile factionFile = new FactionFile();

        public int dataInt;
    }
}
