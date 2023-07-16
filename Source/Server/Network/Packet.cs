using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class Packet
    {
        public string header;

        public string[] contents;

        public Packet(string header, string[] contents = null)
        {
            this.header = header;

            this.contents = contents;
        }
    }
}
