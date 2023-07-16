using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class EventDetailsJSON
    {
        public string eventStepMode;

        public string fromTile;

        public string toTile;

        public string eventID;
    }
}
