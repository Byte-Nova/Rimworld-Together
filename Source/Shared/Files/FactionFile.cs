using System;
using System.Collections.Generic;
using System.Threading;

namespace Shared
{
    [Serializable]
    public class FactionFile
    {
        public string name;

        public List<string> currentMembers = new List<string>();

        public List<int> currentRanks = new List<int>();

        [NonSerialized] public Semaphore savingSemaphore = new Semaphore(1, 1);
    }
}
