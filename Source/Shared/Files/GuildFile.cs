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

        public override string ToString()
        {
            return $"GuildFile:|{Name}|{CurrentUids?.Count ?? 0}|{CurrentLabels?.Count ?? 0}|{CurrentRanks?.Count ?? 0}";
        }
    }
}
