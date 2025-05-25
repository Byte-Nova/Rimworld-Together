using System;

namespace Shared
{
    [Serializable]

    public class HumanFile
    {
        public string ID;

        public string ScribeData;

        public override string ToString()
        {
            return $"HumanFile:|{ID}|{ScribeData?.Length ?? 0}";
        }
    }
}