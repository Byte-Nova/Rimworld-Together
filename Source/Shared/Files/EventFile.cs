using System;

namespace Shared.Files
{
    [Serializable]
    public class EventFile
    {
        public string Name { get; set; } = string.Empty;

        public string DefName { get; set; } = string.Empty;

        public int Cost { get; set; } = 100;

        public bool IsEnabled { get; set; } = true;
    }
}