using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class LoginData
    {
        public string username;

        public string password;

        public string clientVersion;

        public LoginResponse tryResponse;

        public List<string> runningMods = new List<string>();

        public List<string> extraDetails = new List<string>();
    }
}
