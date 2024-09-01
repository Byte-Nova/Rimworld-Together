using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer.Updater
{
    [Serializable]
    public class SettlementFile
    {
        public int tile;

        public string owner;
    }
}
