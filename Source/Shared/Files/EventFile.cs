using System;

namespace Shared
{
    [Serializable]
    public class EventFile
    {
        public string Name;

        public string DefName;

        public int Cost;

        public bool IsEnabled;

        public override string ToString()
        {
            return $"EventFile:|{Name}|{DefName}|{Cost}|{IsEnabled}";
        }
    }
}