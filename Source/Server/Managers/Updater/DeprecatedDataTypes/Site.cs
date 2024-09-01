using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Updater
{
    [Serializable]
    public class SiteFile
    {
        public int tile;

        public string owner;

        public int type;

        public byte[] workerData;

        public bool isFromFaction;

        public string factionName;
    }
}
