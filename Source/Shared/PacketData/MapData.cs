using System;

namespace Shared
{
    [Serializable]

    public class MapData
    {
        //Misc

        public int mapTile;

        public int[] mapSize;

        public string mapOwner;

        public string[] mapMods;

        public string curWeatherDefName;

        //Tiles

        public string[] tileDefNames;

        public string[] tileRoofDefNames;

        public bool[] tilePollutions;

        //Things

        public ThingData[] factionThings;

        public ThingData[] nonFactionThings;

        //Humans

        public HumanData[] factionHumans;

        public HumanData[] nonFactionHumans;

        //Animals

        public AnimalData[] factionAnimals;
        
        public AnimalData[] nonFactionAnimals;
        
    }
}
