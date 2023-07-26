using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class PlayerRecountJSON
    {
        public string currentPlayers;

        public List<string> currentPlayerNames = new List<string>();
    }
}
