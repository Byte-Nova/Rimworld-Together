using System;

namespace Shared
{
    [Serializable]
    public class RoadValuesFile
    {
        //Allowance of the roads

        public bool AllowDirtPath = true;

        public bool AllowDirtRoad = true;

        public bool AllowStoneRoad = true;

        public bool AllowAsphaltPath = true;

        public bool AllowAsphaltHighway = true;

        //Cost of the roads

        public int DirtPathCost = 10;

        public int DirtRoadCost = 20;

        public int StoneRoadCost = 25;

        public int AsphaltPathCost = 30;

        public int AsphaltHighwayCost = 50;
    }
}
