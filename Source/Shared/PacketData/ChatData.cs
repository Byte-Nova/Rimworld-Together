using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class ChatData
    {
        public UserColor userColor;

        public MessageColor messageColor;

        public string username;

        public string message;
    }
}
