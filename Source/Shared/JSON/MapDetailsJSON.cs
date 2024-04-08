using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]

    public class MapDetailsJSON
    {
        //Misc

        public string mapOwner;
        public string mapTile;
        public string mapSize;

        //Mods

        public List<string> mapMods = new List<string>();

        //Tiles

        public List<string> tileDefNames = new List<string>();
        public List<string> tileRoofDefNames = new List<string>();
        public List<bool> tilePollutions = new List<bool>();

        //Things

        public List<ItemDetailsJSON> factionThings = new List<ItemDetailsJSON>();
        public List<ItemDetailsJSON> nonFactionThings = new List<ItemDetailsJSON>();

        //Humans

        public List<HumanDetailsJSON> factionHumans = new List<HumanDetailsJSON>();
        public List<HumanDetailsJSON> nonFactionHumans = new List<HumanDetailsJSON>();

        //Animals

        public List<AnimalDetailsJSON> factionAnimals = new List<AnimalDetailsJSON>();
        public List<AnimalDetailsJSON> nonFactionAnimals = new List<AnimalDetailsJSON>();
    }
}
