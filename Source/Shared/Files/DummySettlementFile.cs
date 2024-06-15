using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class DummySettlementFile
    {
        public int tile;
        public string owner;
        public Goodwill goodwill;
    }
}