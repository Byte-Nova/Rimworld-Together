using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class VisitData
    {
        public int visitStepMode;

        public string visitorName;
        public int fromTile;
        public int targetTile;

        public List<byte[]> mapHumans = new List<byte[]>();
        public List<byte[]> mapAnimals = new List<byte[]>();

        public List<byte[]> caravanHumans = new List<byte[]>();
        public List<byte[]> caravanAnimals = new List<byte[]>();

        public List<string> pawnActionDefNames = new List<string>();
        public List<string> actionTargetA = new List<string>();
        public List<int> actionTargetIndex = new List<int>();
        public List<int> actionTargetType = new List<int>();

        public List<bool> isDrafted = new List<bool>();
        public List<string> positionSync = new List<string>();
        public List<int> rotationSync = new List<int>();

        public int mapTicks;
        public byte[] mapDetails;
        public List<string> mapMods = new List<string>();
    }
}
