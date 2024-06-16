using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineVisitData
    {
        public OnlineVisitStepMode visitStepMode;

        //Map

        public byte[] mapDetails;
        public List<string> mapMods = new List<string>();
        public List<byte[]> mapHumans = new List<byte[]>();
        public List<byte[]> mapAnimals = new List<byte[]>();
        public List<byte[]> caravanHumans = new List<byte[]>();
        public List<byte[]> caravanAnimals = new List<byte[]>();

        //Misc

        public string visitorName;
        public int fromTile;
        public int targetTile;
        public int mapTicks;

        //Orders

        public PawnOrder pawnOrder;
        public CreationOrder creationOrder;
        public DestructionOrder destructionOrder;
    }
}
