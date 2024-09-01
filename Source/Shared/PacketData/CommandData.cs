using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class CommandData
    {
        public CommandMode _commandMode;

        public string _details;
    }
}
