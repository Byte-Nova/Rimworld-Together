using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class LoginData
    {
        public string _username;

        public string _password;

        public string _version;

        public LoginResponse _tryResponse;

        public string[] _runningMods;

        public List<string> _extraDetails = new List<string>();
    }
}
