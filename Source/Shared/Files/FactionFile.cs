using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class FactionFile
    {
        public string name;

        public List<string> currentMembers = new List<string>();

        public List<int> currentRanks = new List<int>();
    }
}
