using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerFactionData
    {
        public FactionManifestMode manifestMode;

        public string manifestData;

        public List<string> manifestComplexData = new List<string>();

        public List<string> manifestSecondaryComplexData = new List<string>();
    }
}
