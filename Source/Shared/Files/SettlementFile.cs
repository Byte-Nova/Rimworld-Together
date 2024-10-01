using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SettlementFile
    {
        public int Tile;

        public string Owner;
        
        public Goodwill Goodwill;

        public bool isShip;
    }

    public class SpaceSettlementFile : SettlementFile
    {
        public float theta;
        public float radius;
        public float phi;
    }

}