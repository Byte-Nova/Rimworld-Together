using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class FactionFile
    {
        public string factionName;

        public List<string> factionMembers = new List<string>();

        public List<string> factionMemberRanks = new List<string>();
    }
}
