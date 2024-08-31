using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerFactionData
    {
        public FactionStepMode _stepMode;

        public FactionFile _factionFile = new FactionFile();

        public int _dataInt;
    }
}
