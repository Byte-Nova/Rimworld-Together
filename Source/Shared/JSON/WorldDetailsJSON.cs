using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class WorldDetailsJSON
    {
        public string worldStepMode;

        public string SeedString;
        public float PlanetCoverage;
        public float Rainfall;
        public float Temperature;
        public float Population;
        public float Pollution;

        public List<string> Factions = new List<string>();
    }
}
