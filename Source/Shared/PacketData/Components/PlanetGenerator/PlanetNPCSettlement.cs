using System;

namespace Shared
{
    [Serializable]
    public class PlanetNPCSettlement
    {
        public int tile;

        public string name;
        
        public string defName;

        public string factionName = ""; // This is only used if there are 2 factions of the same type loaded. It's not null or it would cause errors
    }
}