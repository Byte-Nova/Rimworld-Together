using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class JoinDetailsJSON
    {
        public string username;

        public string password;

        public string tryResponse;

        public string clientVersion;

        public List<string> runningMods = new List<string>();

        public List<string> conflictingMods = new List<string>();
    }
}
