using System;

namespace Shared
{
    [Serializable]
    public class CommandDetailsJSON
    {
        public string commandType;

        public string commandDetails;

        public bool disconnectPlayer;
    }
}
