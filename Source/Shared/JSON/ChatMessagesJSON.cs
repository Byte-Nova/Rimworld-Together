using System;
using System.Collections.Generic;

namespace RimworldTogether.Shared.JSON
{
    [Serializable]
    public class ChatMessagesJSON
    {
        public List<string> userColors = new List<string>();

        public List<string> messageColors = new List<string>();

        public List<string> usernames = new List<string>();

        public List<string> messages = new List<string>();
    }
}
