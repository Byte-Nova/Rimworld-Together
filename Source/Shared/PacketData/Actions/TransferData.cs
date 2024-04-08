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

        public List<byte[]> humanDetailsJSONS = new List<byte[]>();

        public List<byte[]> animalData = new List<byte[]>();

        public List<byte[]> itemDetailsJSONS = new List<byte[]>();
    }
}
