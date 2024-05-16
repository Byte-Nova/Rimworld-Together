using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class ChatData
    {
        public UserColor[] userColors = new UserColor[0];

        public MessageColor[] messageColors = new MessageColor[0];

        public List<string> usernames = new List<string>();

        public List<string> messages = new List<string>();
    }
}
