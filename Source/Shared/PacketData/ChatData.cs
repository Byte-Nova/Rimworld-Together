using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class ChatData
    {
        public List<string> userColors = new List<string>();

        public List<string> messageColors = new List<string>();

        public List<string> usernames = new List<string>();

        public List<string> messages = new List<string>();
    }
}
