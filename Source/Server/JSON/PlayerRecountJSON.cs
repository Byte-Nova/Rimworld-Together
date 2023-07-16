using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class PlayerRecountJSON
    {
        public string currentPlayers;

        public List<string> currentPlayerNames = new List<string>();
    }
}
