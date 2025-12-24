using GameServer.Commands;
using GameServer.Misc;
using Shared.Misc;
using GameServer.Integrations.Discord;
using System;
using System.Linq;

namespace GameServer.Managers
{
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
                    string line = Console.ReadLine();
                    ParseServerCommands(line);
                }
            }
        }

        public static void ParseServerCommands(string command)
        {
            ParseServerCommands(command, false);
        }

        public static void ParseServerCommands(string command, bool fromDiscord)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            string trimmed = command.Trim();

            if (!fromDiscord)
            {
                DiscordBridge.TryRelayConsoleCommandToDiscord(trimmed);
            }
            else
            {
                DiscordBridge.BeginConsoleMirrorWindow();
            }

            string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string parsedPrefix = parts[0].ToLower();
            int parsedParameters = parts.Length - 1;

            commandParameters = parsedParameters > 0 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            try
            {
                CommandBase commandToFetch = ConsoleCommands.Commands.ToList().Find(x => x.Prefix == parsedPrefix);
                if (commandToFetch == null) Printer.Warning($"Command '{parsedPrefix}' was not found");
                else
                {
                    if (commandToFetch.Parameters != parsedParameters && commandToFetch.Parameters != -1)
                    {
                        Printer.Warning($"Command '{commandToFetch.Prefix}' wanted [{commandToFetch.Parameters}] parameters "
                            + $"but was passed [{parsedParameters}]");
                    }
                    else
                    {
                        if (commandToFetch.CommandAction != null) commandToFetch.CommandAction.Invoke();
                        else Printer.Warning($"Command '{commandToFetch.Prefix}' didn't have any action built in");
                    }
                }
            }
            catch (Exception e) { Printer.Error($"Couldn't parse command '{parsedPrefix}'. Reason: {e}"); }
        }
    }
}