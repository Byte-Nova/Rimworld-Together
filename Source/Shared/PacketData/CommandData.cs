using System;

namespace Shared
{
    [Serializable]
    public class CommandData
    {
        public string commandType;

        public string commandDetails;

        public bool disconnectPlayer;
    }
}
