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

        public List<HumanFile> _humans = new List<HumanFile>();

        public List<AnimalFile> _animals = new List<AnimalFile>();

        public List<ThingFile> _things = new List<ThingFile>();
    }
}
