namespace RimworldTogether.GameServer.Misc.Commands
{
    public class ChatCommand
    {
        public string prefix;

        public string description;

        public int parameters;

        public Action commandAction;

        public ChatCommand(string prefix, int parameters, string description, Action commandAction)
        {
            this.prefix = prefix;
            this.parameters = parameters;
            this.description = description;
            this.commandAction = commandAction;
        }
    }
}
