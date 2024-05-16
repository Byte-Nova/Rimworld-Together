using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class TransferData
    {
        public TransferStepMode transferStepMode;

        public TransferMode transferMode;

        public string fromTile;

        public string toTile;

        public List<byte[]> humanDatas = new List<byte[]>();

        public List<byte[]> animalDatas = new List<byte[]>();

        public List<byte[]> itemDatas = new List<byte[]>();
    }
}
