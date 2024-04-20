using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class FactionManifestJSON
    {
        public FactionManifestMode manifestMode;

        public string manifestDetails;

        public List<string> manifestComplexDetails = new List<string>();

        public List<string> manifestSecondaryComplexDetails = new List<string>();
    }
}
