using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class PlayerFactionData
    {
        public string manifestMode;

        public string manifestData;

        public List<string> manifestComplexData = new List<string>();

        public List<string> manifestSecondaryComplexData = new List<string>();
    }
}
