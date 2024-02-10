using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class StructureLikelihoodJSON
    {
        public string tile;

        public string owner;

        public string likelihood;

        public List<string> settlementTiles = new List<string>();
        public List<string> settlementLikelihoods = new List<string>();

        public List<string> siteTiles = new List<string>();
        public List<string> siteLikelihoods = new List<string>();
    }
}
