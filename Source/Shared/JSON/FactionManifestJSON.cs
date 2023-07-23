using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class FactionManifestJSON
    {
        public string manifestMode;

        public string manifestDetails;

        public List<string> manifestComplexDetails = new List<string>();

        public List<string> manifestSecondaryComplexDetails = new List<string>();
    }
}
