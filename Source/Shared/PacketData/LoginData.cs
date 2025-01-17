using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class LoginData
    {
        public string _uid;

        public string _username;

        public string _version;

        public ModConfigFile _runningMods;

        public LoginResponse _tryResponse;

        public List<string> _extraDetails = new List<string>();
    }
}
