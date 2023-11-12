using System;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class SaveFileJSON
    {
        public string saveMode;

        public byte[] saveData;
    }
}
