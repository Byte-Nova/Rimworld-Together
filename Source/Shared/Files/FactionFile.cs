using System;
using System.Collections.Generic;
using System.Threading;

namespace Shared
{
    [Serializable]
    public class FactionFile
    {
        public string Name;

        public List<string> CurrentMembers = new List<string>();

        public List<int> CurrentRanks = new List<int>();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
