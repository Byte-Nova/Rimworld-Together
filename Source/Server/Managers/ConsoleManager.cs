using GameServer.Commands;
using GameServer.Misc;
using Shared;

namespace GameServer.Managers
{
    [RTManager]
    public static class ConsoleManager
    {
        public static string[] commandParameters;

        public static void ListenForServerCommands()
        {
            bool interactiveConsole = false;

            try { interactiveConsole = Console.In.Peek() != -1 ? true : false; }
            catch { Printer.Warning($"Couldn't find interactive console, disabling commands"); }

            if (interactiveConsole)
            {
                while (true)
                {
                    ParseServerCommands(Console.ReadLine());
                }
            }
        }

        public static void ParseServerCommands(string command)
        {
            string parsedPrefix = command.Split(' ')[0].ToLower();
            int parsedParameters = command.Split(' ').Count() - 1;
            commandParameters = command.Replace(parsedPrefix + " ", "").Split(" ");

            try
            {
                BaseServerCommand commandToFetch = ConsoleCommands.commands.ToList().Find(x => x.prefix == parsedPrefix);
                if (commandToFetch == null) Printer.Warning($"Command '{parsedPrefix}' was not found");
                else
                {
                    if (commandToFetch.parameters != parsedParameters && commandToFetch.parameters != -1)
                    {
                        Printer.Warning($"Command '{commandToFetch.prefix}' wanted [{commandToFetch.parameters}] parameters "
                            + $"but was passed [{parsedParameters}]");
                    }

                    else
                    {
                        if (commandToFetch.commandAction != null) commandToFetch.commandAction.Invoke();

                        else Printer.Warning($"Command '{commandToFetch.prefix}' didn't have any action built in");
                    }
                }
            }
            catch (Exception e) { Printer.Error($"Couldn't parse command '{parsedPrefix}'. Reason: {e}"); }
        }
    }
}