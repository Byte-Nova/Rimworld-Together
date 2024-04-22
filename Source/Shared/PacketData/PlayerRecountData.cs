using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class PlayerRecountData
    {
        public string currentPlayers;

        public List<string> currentPlayerNames = new List<string>();
    }
}
