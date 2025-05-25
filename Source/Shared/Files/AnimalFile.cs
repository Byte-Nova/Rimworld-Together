using System;

namespace Shared
{
    [Serializable]

    public class AnimalFile
    {
        public string ID;

        public string ScribeData;

        public override string ToString()
        {
            return $"AnimalFile:|{ID}|{ScribeData?.Length ?? 0}";
        }
    }
}