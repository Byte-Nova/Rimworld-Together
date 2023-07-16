using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class VisitDetailsJSON
    {
        public string visitStepMode;

        public string visitorName;

        public string fromTile;

        public string targetTile;

        public List<string> mapHumans = new List<string>();
        public List<string> mapAnimals = new List<string>();

        public List<string> caravanHumans = new List<string>();
        public List<string> caravanAnimals = new List<string>();

        public List<string> pawnActionDefNames = new List<string>();
        public List<string> actionTargetA = new List<string>();
        public List<string> actionTargetType = new List<string>();

        public List<string> pawnPositions = new List<string>();

        public List<string> mapMods = new List<string>();

        public string deflatedMapData;
    }
}
