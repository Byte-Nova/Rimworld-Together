using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class PlayerRecountData
    {
        public string _currentPlayers;

        public List<string> _currentPlayerNames = new List<string>();
    }
}
