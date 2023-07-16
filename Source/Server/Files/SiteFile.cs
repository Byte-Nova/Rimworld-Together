using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class SiteFile
    {
        public string tile;

        public string owner;

        public string type;

        public string workerData;

        public bool isFromFaction;

        public string factionName;
    }
}
