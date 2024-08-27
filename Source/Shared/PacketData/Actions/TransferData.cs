using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class TransferData
    {
        public TransferStepMode stepMode;

        public TransferMode transferMode;

        public int fromTile;

        public int toTile;

        public List<HumanData> humanDatas = new List<HumanData>();

        public List<AnimalData> animalDatas = new List<AnimalData>();

        public List<ThingData> itemDatas = new List<ThingData>();
    }
}
