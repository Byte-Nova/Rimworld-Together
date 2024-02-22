using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class MapDetailsJSON
    {
        public string mapOwner;
        public string mapTile;
        public string mapSize;

        public List<string> mapMods = new List<string>();

        public List<string> tileDefNames = new List<string>();
        public List<string> roofDefNames = new List<string>();

        public List<string> itemDetailsJSONS = new List<string>();
        public List<string> playerItemDetailsJSON = new List<string>();

        public List<string> humanDetailsJSONS = new List<string>();
        public List<string> playerHumanDetailsJSON = new List<string>();

        public List<string> animalDetailsJSON = new List<string>();
        public List<string> playerAnimalDetailsJSON = new List<string>();
    }
}
