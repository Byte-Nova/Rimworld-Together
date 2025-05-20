using System;
using System.IO;
using System.Linq;
using GameServer.Commands;
using GameServer.Core;
using GameServer.Misc;

namespace GameServer.Managers
{
    public static class ConsoleManager
    {
        public static string[] CommandParameters;
        [Obsolete("Use ConsoleManager.CommandParameters instead.")]
        public static string[] commandParameters
        {
            get => CommandParameters;
            set => CommandParameters = value;
        }

        public static void ListenForServerCommands()
        {
            bool interactive = false;
            try { interactive = Console.In.Peek() != -1; }
            catch { Printer.Warning("Couldn't find interactive console, disabling commands"); }

            if (!interactive) return;

            while (true)
                ParseServerCommands(Console.ReadLine() ?? "");
        }

        public static void ParseServerCommands(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)) return;

            var parts  = cmd.Split(' ');
            var prefix = parts[0].ToLowerInvariant();
            var count  = parts.Length - 1;

            CommandParameters = cmd.Replace(parts[0] + " ", "")
                                   .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var command = ConsoleCommands.Commands
                                             .FirstOrDefault(c => c.Prefix.Equals(prefix,
                                                                                   StringComparison.OrdinalIgnoreCase));

                if (command == null)
                {
                    Printer.Warning($"Command '{prefix}' was not found");
                }
                else if (command.Parameters != count && command.Parameters != -1)
                {
                    Printer.Warning($"Command '{command.Prefix}' wanted [{command.Parameters}] parameters but got [{count}]");
                }
                else
                {
                    command.CommandAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Printer.Error($"Couldn't parse command '{prefix}'. Reason: {ex.Message}");
            }
        }

        public static void ProcessDiscordCommand(string cmd, string user)
        {
            if (Master.DiscordConfig?.Enabled != true) return;

            // 1) echo in local console
            Printer.Message($"[Discord Console] {user}: {cmd}");

            // 2) capture subsequent Printer output
            Printer.StartDiscordBuffer(user);

            // 3) parse as if it had been typed in the server console
            ParseServerCommands(cmd);

            // 4) collect captured lines
            var lines = Printer.FlushDiscordBuffer();
            if (lines.Count == 0) lines.Add("*(no output)*");

            // 5) prepend bold header
            lines.Insert(0, $"**{user}: {cmd}**");

            // 6) ship to Discord
            var payload = string.Join('\n', lines);
            _ = DiscordManager.SendConsoleMessageAsync(payload);
        }
    }
}