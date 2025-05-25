using System;

namespace Shared
{
    [Serializable]

    public class ThingFile
    {
        public string ID;

        public string ScribeData;

        public override string ToString()
        {
            return $"ThingFile:|{ID}|{ScribeData?.Length ?? 0}";
        }
    }
}