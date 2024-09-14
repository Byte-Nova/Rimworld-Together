using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class TransferData
    {
        public TransferStepMode _stepMode;

        public TransferMode _transferMode;

        public int _fromTile;

        public int _toTile;

        public List<HumanDataFile> _humans = new List<HumanDataFile>();

        public List<AnimalDataFile> _animals = new List<AnimalDataFile>();

        public List<ThingDataFile> _things = new List<ThingDataFile>();
    }
}
