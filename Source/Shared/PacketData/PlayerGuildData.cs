using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class PlayerGuildData
    {
        public GuildStepMode _stepMode;

        public GuildFile _file = new GuildFile();

        public int _dataInt;
    }
}
