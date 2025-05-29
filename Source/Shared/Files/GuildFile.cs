using System;
using System.Collections.Generic;
using System.Threading;

namespace Shared
{
    [Serializable]
    public class GuildFile
    {
        public string Name;

        public List<string> CurrentUids = new List<string>();

        public List<string> CurrentLabels = new List<string>();

        public List<int> CurrentRanks = new List<int>();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
