using System;

namespace Shared
{
    [Serializable]
    public class PlanetNPCSettlementDetails
    {
        public int Tile { get; set; }

        public string Name { get; set; }

        public string DefName { get; set; }

        // This is only used if there are 2 factions of the same type loaded. It's not null or it would cause errors

        public string FactionName { get; set; } = ""; 
    }
}