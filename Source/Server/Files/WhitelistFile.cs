using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class WhitelistFile
    {
        public bool UseWhitelist = false;

        public List<string> WhitelistedUsers = new List<string>() { };
    }
}
