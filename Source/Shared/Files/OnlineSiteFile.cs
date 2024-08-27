using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineSiteFile
    {
        public int tile;

        public string owner;

        public Goodwill goodwill;

        public int type;
        
        public bool fromFaction;
    }
}