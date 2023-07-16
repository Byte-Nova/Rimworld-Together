using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class ServerConfigFile
    {
        public string IP = "127.0.0.1";

        public string Port = "25555";

        public string MaxPlayers = "100";
    }
}
