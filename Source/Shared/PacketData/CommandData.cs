using System;
using System.Data;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class CommandData
    {
        public CommandName commandType;

        public string commandDetails;
    }
}
