namespace GameServer.Commands
{
    public class CommandBase
    {
        public string Prefix { get; private set; }

        public string Description { get; private set; }

        public int Parameters { get; private set; }

        public Action CommandAction { get; private set; }

        public CommandBase(string prefix, int parameters, string description, Action action)
        {
            this.Prefix = prefix;
            this.Parameters = parameters;
            this.Description = description;
            this.CommandAction = action;
        }
    }
}
