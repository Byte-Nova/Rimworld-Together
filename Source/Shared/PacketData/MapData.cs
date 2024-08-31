using System;

namespace Shared
{
    [Serializable]

    public class MapData
    {
        //Misc

        public int _mapTile;

        public int[] _mapSize;

        public string _mapOwner;

        public string[] _mapMods;

        public string _curWeatherDefName;

        //Tiles

        public string[] _tileDefNames;

        public string[] _tileRoofDefNames;

        public bool[] _tilePollutions;

        //Things

        public ThingData[] _factionThings;

        public ThingData[] _nonFactionThings;

        //Humans

        public HumanData[] _factionHumans;

        public HumanData[] _nonFactionHumans;

        //Animals

        public AnimalData[] _factionAnimals;
        
        public AnimalData[] _nonFactionAnimals;
        
    }
}
