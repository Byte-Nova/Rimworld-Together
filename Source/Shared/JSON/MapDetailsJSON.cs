using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class MapDetailsJSON
    {
        public string mapOwner;
        public string mapTile;
        public string mapSize;

        //list of mods on Host(Settlement) map
        public List<string> mapMods = new List<string>();

        public List<string> tileDefNames = new List<string>();
        public List<string> roofDefNames = new List<string>();

        //list of Caravan's items
        public List<string> itemDetailsJSONS = new List<string>();
        //list of Settlement's items
        public List<string> playerItemDetailsJSON = new List<string>();

        //list of Caravan's humans
        public List<string> humanDetailsJSONS = new List<string>();
        //list of Settlement's humans
        public List<string> playerHumanDetailsJSON = new List<string>();
        
        //list of Caravan's animals
        public List<string> animalDetailsJSON = new List<string>();
        //list of Settlement's animals
        public List<string> playerAnimalDetailsJSON = new List<string>();
    }
}
