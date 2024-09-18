using Shared;

namespace GameServer
{
    public static class CommandManager
    {
        public static string[] commandParameters;

        public static void ParseServerCommands(string parsedString)
        {
            string parsedPrefix = parsedString.Split(' ')[0].ToLower();
            int parsedParameters = parsedString.Split(' ').Count() - 1;
            commandParameters = parsedString.Replace(parsedPrefix + " ", "").Split(" ");

            try
            {
                ServerCommand commandToFetch = CommandStorage.serverCommands.ToList().Find(x => x.prefix == parsedPrefix);
                if (commandToFetch == null) Logger.Warning($"Command '{parsedPrefix}' was not found");
                else
                {
                    if (commandToFetch.parameters != parsedParameters && commandToFetch.parameters != -1)
                    {
                        Logger.Warning($"Command '{commandToFetch.prefix}' wanted [{commandToFetch.parameters}] parameters "
                            + $"but was passed [{parsedParameters}]");
                    }

                    else
                    {
                        if (commandToFetch.commandAction != null) commandToFetch.commandAction.Invoke();

                        else Logger.Warning($"Command '{commandToFetch.prefix}' didn't have any action built in");
                    }
                }
            }
            catch (Exception e) { Logger.Error($"Couldn't parse command '{parsedPrefix}'. Reason: {e}"); }
        }

        public static void ListenForServerCommands()
        {
            bool interactiveConsole = false;

            try { interactiveConsole = Console.In.Peek() != -1 ? true : false; }
            catch { Logger.Warning($"Couldn't find interactive console, disabling commands"); }

            if (interactiveConsole)
            {
                while (true)
                {
                    ParseServerCommands(Console.ReadLine());
                }
            }
        }
    }
}