using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class TransferManifestJSON
    {
        public string transferStepMode;

        public string transferMode;

        public string fromTile;

        public string toTile;

        public List<byte[]> humanDetailsJSONS = new List<byte[]>();

        public List<byte[]> animalDetailsJSON = new List<byte[]>();

        public List<byte[]> itemDetailsJSONS = new List<byte[]>();
    }
}
