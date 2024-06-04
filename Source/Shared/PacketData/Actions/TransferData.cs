using System;
using System.Collections.Generic;
using Shared;

namespace Shared
{
    [Serializable]
    public class TransferData
    {
        public CommonEnumerators.TransferStepMode transferStepMode;

        public CommonEnumerators.TransferMode transferMode;

        public string fromTile;

        public string toTile;

        public List<byte[]> humanDatas = new List<byte[]>();

        public List<byte[]> animalDatas = new List<byte[]>();

        public List<byte[]> itemDatas = new List<byte[]>();
    }
}
