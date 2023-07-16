using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class LoginDetailsJSON
    {
        public string username;

        public string password;

        public string tryResponse;

        public string clientVersion;

        public List<string> runningMods = new List<string>();

        public List<string> conflictingMods = new List<string>();
    }
}
