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
        public static string DiscordConsoleUser { get; private set; }
        private static readonly List<string> DiscordConsoleBuffer = new();

        private static readonly Semaphore Semaphore = new(1, 1);
        private static readonly Dictionary<LogMode, ConsoleColor> ColorDictionary = new()
        {
            { LogMode.Message,  ConsoleColor.White   },
            { LogMode.Warning,  ConsoleColor.Yellow  },
            { LogMode.Error,    ConsoleColor.Red     },
            { LogMode.Title,    ConsoleColor.Green   },
            { LogMode.Outsider, ConsoleColor.Magenta }
        };

        // façade helpers
        public static void Message (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Message,  i);
        public static void Warning (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Warning,  i);
        public static void Error   (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Error,    i);
        public static void Title   (object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Title,    i);
        public static void Outsider(object v, LogImportanceMode i = LogImportanceMode.Normal)
            => Write(v?.ToString(), LogMode.Outsider, i);

        // Discord capture 
        public static void StartDiscordBuffer(string username)
        {
            DiscordConsoleUser = username;
            DiscordConsoleBuffer.Clear();
        }

        public static List<string> FlushDiscordBuffer()
        {
            var result = new List<string>(DiscordConsoleBuffer);
            DiscordConsoleBuffer.Clear();
            DiscordConsoleUser = null!;
            return result;
        }

        // core writer
        private static void Write(string? text, LogMode mode, LogImportanceMode importance, bool writeToLogs = true)
        {
            if (text == null) return;
            Semaphore.WaitOne();
            try
            {
                if (!ShouldPrint(importance)) return;

                if (writeToLogs)
                    WriteToLogs(text);

                var ts = DateTime.Now.ToString("HH:mm:ss");
                Console.ForegroundColor = ColorDictionary[mode];
                Console.WriteLine($"[{ts}] | {text}");
                Console.ForegroundColor = ConsoleColor.White;

                // only buffer while capturing a Discord-command response
                if (!string.IsNullOrEmpty(DiscordConsoleUser) && Master.DiscordConfig?.Enabled == true)
                    DiscordConsoleBuffer.Add(text);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private static void WriteToLogs(string toLog)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append($"[{DateTime.Now:HH:mm:ss}] | {toLog}{Environment.NewLine}");
                var fname = $"{DateTime.Now:yyyy-MM-dd}.txt";
                var path  = Path.Combine(Master.SystemLogsPath, fname);
                File.AppendAllText(path, sb.ToString());
            }
            catch { /* swallow */ }
        }

        private static bool ShouldPrint(LogImportanceMode importance) =>
            importance switch
            {
                LogImportanceMode.Normal  => true,
                LogImportanceMode.Verbose => Master.ServerConfig.VerboseLogs,
                LogImportanceMode.Extreme => Master.ServerConfig.ExtremeVerboseLogs,
                _ => false
            };
    }
}