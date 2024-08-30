using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class ChatData
    {
        public UserColor _usernameColor;

        public MessageColor _messageColor;

        public string _username;

        public string _message;
    }
}
