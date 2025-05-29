using static Shared.CommonEnumerators;

namespace Shared
{

    public class ChatData
    {
        public UserColor _usernameColor { get; set; } = UserColor.Normal;

        public MessageColor _messageColor { get; set; } = MessageColor.Normal;

        public string _username { get; set; } = string.Empty;

        public string _message { get; set; } = string.Empty;
    }
}
