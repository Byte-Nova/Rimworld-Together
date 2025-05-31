using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using GameServer.Core;
using static Shared.CommonEnumerators;

namespace GameServer.Misc
{
    public static class Printer
    {
        public enum LogKind { Title, Warning, Error }
        public static event Action<LogKind, string>? ConsoleTap;

        public static string DiscordConsoleUser { get; private set; } = string.Empty;
        private static readonly List<string> DiscordConsoleBuffer = new();

        private static readonly Semaphore Semaphore = new(1, 1);
        private static readonly Dictionary<LogMode, ConsoleColor> ColorDictionary = new()
        {
            { LogMode.Message,  ConsoleColor.White   },
            { LogMode.Warning,  ConsoleColor.Yellow  },
            { LogMode.Error,    ConsoleColor.Red     },
            { LogMode.Title,    ConsoleColor.Green   },
            { LogMode.Discord,  ConsoleColor.Magenta }
        };

        // façade helpers
        public static void Message (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Message,  i);
        public static void Warning (object v, LogImportanceMode i = LogImportanceMode.Normal)
        {
            Write(v?.ToString(), LogMode.Warning, i);
            ConsoleTap?.Invoke(LogKind.Warning, v?.ToString() ?? string.Empty);   // NEW
        }

        public static void Error   (object v, LogImportanceMode i = LogImportanceMode.Normal)
        {
            Write(v?.ToString(), LogMode.Error,   i);
            ConsoleTap?.Invoke(LogKind.Error,   v?.ToString() ?? string.Empty);   // NEW
        }

        public static void Title   (object v, LogImportanceMode i = LogImportanceMode.Normal)
        {
            Write(v?.ToString(), LogMode.Title,   i);
            ConsoleTap?.Invoke(LogKind.Title,   v?.ToString() ?? string.Empty);   // NEW
        }

        public static void Discord (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Discord, i);

        public static void StartDiscordBuffer(string username)
        {
            DiscordConsoleUser = username;
            DiscordConsoleBuffer.Clear();
        }

        public static List<string> FlushDiscordBuffer()
        {
            var outp = new List<string>(DiscordConsoleBuffer);
            DiscordConsoleBuffer.Clear();
            DiscordConsoleUser = string.Empty;
            return outp;
        }

        private static void Write(string? text,
                                  LogMode mode,
                                  LogImportanceMode importance,
                                  bool writeToLogs = true)
        {
            if (text == null) return;
            Semaphore.WaitOne();
            try
            {
                if (!ShouldPrint(importance)) return;

                if (writeToLogs) WriteToLogs(text);

                var ts = DateTime.Now.ToString("HH:mm:ss");
                Console.ForegroundColor = ColorDictionary[mode];
                Console.WriteLine($"[{ts}] | {text}");
                Console.ForegroundColor = ConsoleColor.White;

                // capture console-command output for Discord
                if (!string.IsNullOrEmpty(DiscordConsoleUser) &&
                    Master.DiscordConfig?.Enabled == true)
                    DiscordConsoleBuffer.Add(text);
            }
            finally { Semaphore.Release(); }
        }

        private static void WriteToLogs(string line)
        {
            try
            {
                var path = Path.Combine(Master.SystemLogsPath,
                                        $"{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(path,
                    $"[{DateTime.Now:HH:mm:ss}] | {line}{Environment.NewLine}");
            }
            catch { /* swallow */ }
        }

        private static bool ShouldPrint(LogImportanceMode imp) =>
            imp switch
            {
                LogImportanceMode.Normal  => true,
                LogImportanceMode.Verbose => Master.ServerConfig.VerboseLogs,
                LogImportanceMode.Extreme => Master.ServerConfig.ExtremeVerboseLogs,
                _ => false
            };
    }
}