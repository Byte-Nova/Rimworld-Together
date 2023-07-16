using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class MapDetailsJSON
    {
        public string mapTile;

        public string mapSize;

        public List<string> tileDefNames = new List<string>();

        public List<string> roofDefNames = new List<string>();

        public List<string> itemDetailsJSONS = new List<string>();
        public List<string> playerItemDetailsJSON = new List<string>();

        public List<string> humanDetailsJSONS = new List<string>();
        public List<string> playerHumanDetailsJSON = new List<string>();

        public List<string> animalDetailsJSON = new List<string>();
        public List<string> playerAnimalDetailsJSON = new List<string>();

        public List<string> mapMods = new List<string>();

        public string deflatedMapData;
    }
}
