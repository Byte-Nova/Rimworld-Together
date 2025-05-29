using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{

    public class LoginData
    {
        public string _uid { get; set; } = string.Empty;

        public string _username { get; set; } = string.Empty;

        public ModConfigFile _runningMods { get; set; } = null;

        public LoginResponse _tryResponse { get; set; } = LoginResponse.InvalidLogin;

        public List<string> _extraDetails { get; set; } = new List<string>();
    }
}
