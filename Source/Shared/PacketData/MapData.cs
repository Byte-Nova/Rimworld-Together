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

        public ThingDataFile[] _factionThings;

        public ThingDataFile[] _nonFactionThings;

        //Humans

        public HumanDataFile[] _factionHumans;

        public HumanDataFile[] _nonFactionHumans;

        //Animals

        public AnimalDataFile[] _factionAnimals;
        
        public AnimalDataFile[] _nonFactionAnimals;
        
    }
}
