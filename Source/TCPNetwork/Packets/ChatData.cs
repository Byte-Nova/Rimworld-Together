using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class ChatData
    {
        public UserColor _usernameColor { get; set; } = UserColor.Normal;

        public MessageColor _messageColor { get; set; } = MessageColor.Normal;

        public string _username { get; set; } = string.Empty;

        public string _message { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ChatData:|{_usernameColor}|{_messageColor}|{_username}|{_message}";
        }
    }
}
