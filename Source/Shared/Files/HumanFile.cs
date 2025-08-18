using System;

namespace Shared.Files
{
    [Serializable]

    public class HumanFile
    {
        public string ScribeData { get; set; } = string.Empty;

        public string IdeologyData { get; set; } = string.Empty;
    }
}