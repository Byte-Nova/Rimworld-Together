using System;
using System.IO;
using System.Text;
using System.Threading;
using GameServer.Core;
using static Shared.CommonEnumerators;
using GameServer.Managers.External;

namespace GameServer.Misc
{
    public static class Printer
    {
        private static readonly Semaphore semaphore = new Semaphore(1, 1);

        private static readonly Dictionary<LogMode, ConsoleColor> colorDictionary = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Message,  ConsoleColor.White },
            { LogMode.Warning,  ConsoleColor.Yellow },
            { LogMode.Error,    ConsoleColor.Red },
            { LogMode.Title,    ConsoleColor.Green },
            { LogMode.Outsider, ConsoleColor.Magenta }
        };

        public static void Message(object value, LogImportanceMode importance = LogImportanceMode.Normal)
        {
            WriteToConsole(value.ToString(), LogMode.Message, importance);
        }

        public static void Warning(object value, LogImportanceMode importance = LogImportanceMode.Normal)
        {
            WriteToConsole(value.ToString(), LogMode.Warning, importance);
        }

        public static void Error(object value, LogImportanceMode importance = LogImportanceMode.Normal)
        {
            WriteToConsole(value.ToString(), LogMode.Error, importance);
        }

        public static void Title(object value, LogImportanceMode importance = LogImportanceMode.Normal)
        {
            WriteToConsole(value.ToString(), LogMode.Title, importance);
        }

        public static void Outsider(object value, LogImportanceMode importance = LogImportanceMode.Normal)
        {
            WriteToConsole(value.ToString(), LogMode.Outsider, importance);
        }

        private static void WriteToConsole(string text, LogMode mode, LogImportanceMode importance, bool writeToLogs = true)
        {
            semaphore.WaitOne();
            try
            {
                if (CheckIfShouldPrint(importance))
                {
                    if (writeToLogs)
                        WriteToLogs(text);

                    // Enqueue for Discord console channel merging.
                    DiscordManager.EnqueueConsoleLine($"[{mode}] {text}");

                    Console.ForegroundColor = colorDictionary[mode];
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | {text}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void WriteToLogs(string toLog)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:HH:mm:ss}] | {toLog}");
            sb.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = $"{dateTime.Year}-{dateTime.Month:D2}-{dateTime.Day:D2}";
            string nowFullPath = Path.Combine(Master.systemLogsPath, nowFileName + ".txt");

            File.AppendAllText(nowFullPath, sb.ToString());
        }

        private static bool CheckIfShouldPrint(LogImportanceMode importance)
        {
            if (importance == LogImportanceMode.Normal)
                return true;
            if (importance == LogImportanceMode.Verbose && Master.serverConfig.VerboseLogs)
                return true;
            if (importance == LogImportanceMode.Extreme && Master.serverConfig.ExtremeVerboseLogs)
                return true;
            return false;
        }
    }
}