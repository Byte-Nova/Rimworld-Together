using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
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
