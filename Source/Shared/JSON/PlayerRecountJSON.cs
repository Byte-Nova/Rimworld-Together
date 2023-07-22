using System;
using System.Collections.Generic;

namespace Shared.JSON
{
    [Serializable]
    public class PlayerRecountJSON
    {
        public string currentPlayers;

        public List<string> currentPlayerNames = new List<string>();
    }
}
