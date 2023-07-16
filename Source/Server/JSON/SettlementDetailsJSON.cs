using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class SettlementDetailsJSON
    {
        public string settlementStepMode;

        public string tile;

        public string owner;

        public string value;
    }
}
