using System;
using System.Collections.Generic;
using System.Threading;

namespace Shared.Files
{
    [Serializable]
    public class GuildFile
    {
        public string Name { get; set; } = string.Empty;

        public List<string> CurrentUids { get; set; } = new List<string>();

        public List<string> CurrentLabels { get; set; } = new List<string>();

        public List<int> CurrentRanks { get; set; } = new List<int>();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
