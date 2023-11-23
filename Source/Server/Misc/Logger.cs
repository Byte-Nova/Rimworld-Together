using System.Text;
using RimworldTogether.GameServer.Core;

namespace RimworldTogether.GameServer.Misc
{
    public static class Logger
    {
        public enum LogMode { Normal, Warning, Error, Title }

        public static Dictionary<LogMode, ConsoleColor> colorDictionary = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Normal, ConsoleColor.White },
            { LogMode.Warning, ConsoleColor.Yellow },
            { LogMode.Error, ConsoleColor.Red },
            { LogMode.Title, ConsoleColor.Green }
        };

        private static bool isLogging;

        public static void WriteToConsole(string text, LogMode mode = LogMode.Normal, bool writeToLogs = true)
        {
            while (isLogging) Thread.Sleep(100);

            isLogging = true;

            if (writeToLogs) WriteToLogs(text);

            Console.ForegroundColor = colorDictionary[mode];
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | " + text);
            Console.ForegroundColor = ConsoleColor.White;

            isLogging = false;
        }

        private static void WriteToLogs(string toLog)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | " + toLog);
            stringBuilder.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = (dateTime.Month + "-" + dateTime.Day + "-" + dateTime.Year).ToString();
            string nowFullPath = Program.logsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";

            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }
}
