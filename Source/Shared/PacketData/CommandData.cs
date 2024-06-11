using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class CommandData
    {
        public CommandMode commandMode;

        public string commandDetails;
    }
}
