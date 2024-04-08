using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class TransferData
    {
        public string transferStepMode;

        public string transferMode;

        public string fromTile;

        public string toTile;

        public List<byte[]> humanDatas = new List<byte[]>();

        public List<byte[]> animalDatas = new List<byte[]>();

        public List<byte[]> itemDatas = new List<byte[]>();
    }
}
