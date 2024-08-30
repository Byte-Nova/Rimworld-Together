using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineSettlementFile
    {
        public int tile;

        public string owner;
        
        public Goodwill goodwill;

        public bool isShip = false;
    }

    public class OnlineSpaceSettlementFile : OnlineSettlementFile 
    {
        public float radius;
        public float phi;
        public float theta;
    }

}