namespace GameServer
{
    public class ChatCommand
    {
        public string prefix;

        public string description;

        public int parameters;

        public string arguments;

        public bool adminOnly;

        public Action commandAction;

        public ChatCommand(string prefix, int parameters, string description, bool adminOnly, Action commandAction,
            string arguments = "")
        {
            this.prefix = prefix;
            this.description = description;
            this.parameters = parameters;
            this.arguments = arguments;
            this.adminOnly = adminOnly;
            this.commandAction = commandAction;
        }
    }
}