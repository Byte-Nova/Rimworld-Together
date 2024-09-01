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

        public List<HumanData> _humans = new List<HumanData>();

        public List<AnimalData> _animals = new List<AnimalData>();

        public List<ThingData> _things = new List<ThingData>();
    }
}
