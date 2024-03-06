using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class VisitDetailsJSON
    {
        public int visitStepMode;

        public string visitorName;
        public string fromTile;
        public string targetTile;

        public List<string> mapHumans = new List<string>();
        public List<string> mapAnimals = new List<string>();

        public List<string> caravanHumans = new List<string>();
        public List<string> caravanAnimals = new List<string>();

        public List<string> pawnActionDefNames = new List<string>();
        public List<string> actionTargetA = new List<string>();
        public List<int> actionTargetType = new List<int>();

        public List<bool> isDrafted = new List<bool>();
        public List<string> positionSync = new List<string>();
        public List<int> rotationSync = new List<int>();

        public int mapTicks;
        public byte[] mapDetails;
        public List<string> mapMods = new List<string>();
    }
}
