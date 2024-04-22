using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class LoginData
    {
        public string username;

        public string password;

        public string tryResponse;

        public string clientVersion;

        public List<string> runningMods = new List<string>();

        public List<string> extraDetails = new List<string>();
    }
}
