using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Commands;
using GameServer.Core;
using GameServer.Misc;

namespace GameServer.Managers
{
    public static class ConsoleManager
    {
        public static string[] CommandParameters { get; private set; } = Array.Empty<string>();

        [Obsolete("Use ConsoleManager.CommandParameters instead.")]
        public static string[] commandParameters
        {
            get => CommandParameters;
            set => CommandParameters = value;
        }

        public static Task ListenForServerCommandsAsync(CancellationToken token = default)
            => Task.Run(() => ListenLoopAsync(token), token);

        public static void ListenForServerCommands()
            => ListenLoopAsync(CancellationToken.None).GetAwaiter().GetResult();

        private static async Task ListenLoopAsync(CancellationToken token)
        {
            if (!ConsoleIsInteractive())
            {
                Printer.Warning("Interactive console not detected – command loop disabled");
                return;
            }

            while (!token.IsCancellationRequested && !Master.IsClosing)
            {
                string? line;
                try { line = await Console.In.ReadLineAsync(); }
                catch { break; }                     // stdin closed

                if (line != null) ParseServerCommands(line);
            }
        }

        private static bool ConsoleIsInteractive()
        {
            try { return Console.In.Peek() != -1; }
            catch { return false; }
        }

        public static void ParseServerCommands(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)) return;

            var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var prefix = parts[0].ToLowerInvariant();
            var argCount = parts.Length - 1;
            CommandParameters = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            try
            {
                var match = ConsoleCommands.Commands
                                           .FirstOrDefault(c => c.Prefix.Equals(prefix,
                                                           StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    Printer.Warning($"Command '{prefix}' was not found");
                    return;
                }

                if (match.Parameters != argCount && match.Parameters != -1)
                {
                    Printer.Warning($"Command '{match.Prefix}' wanted [{match.Parameters}] parameters but got [{argCount}]");
                    return;
                }

                match.CommandAction?.Invoke();
            }
            catch (Exception ex)
            {
                Printer.Error($"Exception while executing '{prefix}': {ex.Message}");
            }
        }

        public static void ProcessDiscordCommand(string cmd, string user)
        {
            if (Master.DiscordConfig?.Enabled != true) return;

            Printer.Message($"[Discord Console] {user}: {cmd}");
            Printer.StartDiscordBuffer(user);

            ParseServerCommands(cmd);                 // run command

            var lines = Printer.FlushDiscordBuffer();
            if (lines.Count == 0) lines.Add("*(no output)*");
            lines.Insert(0, $"**{user}: {cmd}**");

            _ = DiscordManager.SendConsoleMessageAsync(string.Join('\n', lines));
        }
    }
}