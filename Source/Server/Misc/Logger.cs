using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class Logger
    {
        //Variables

        public static Semaphore semaphore = new Semaphore(1, 1);

        private static Dictionary<LogMode, ConsoleColor> colorDictionary = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Message, ConsoleColor.White },
            { LogMode.Warning, ConsoleColor.Yellow },
            { LogMode.Error, ConsoleColor.Red },
            { LogMode.Title, ConsoleColor.Green },
            { LogMode.Outsider, ConsoleColor.Magenta}
        };

        //Wrapper to write log in white color

        public static void Message(string message) { WriteToConsole(message, LogMode.Message); }

        //Wrapper to write log in yellow color

        public static void Warning(string message) { WriteToConsole(message, LogMode.Warning); }

        //Wrapper to write log in red color

        public static void Error(string message) { WriteToConsole(message, LogMode.Error); }

        //Wrapper to write log in green color

        public static void Title(string message) { WriteToConsole(message, LogMode.Title); }

        //Wrapper to write log in X color

        public static void Outsider(string message) { WriteToConsole(message, LogMode.Outsider); }

        //Actual function that writes to the console

        private static void WriteToConsole(string text, LogMode mode = LogMode.Message, bool writeToLogs = true)
        {
            semaphore.WaitOne();

            if (writeToLogs) WriteToLogs(text);

            Console.ForegroundColor = colorDictionary[mode];
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | " + text);
            Console.ForegroundColor = ConsoleColor.White;

            if (Master.discordConfig != null && Master.discordConfig.Enabled) DiscordManager.SendMessageToConsoleChannelBuffer(text);

            semaphore.Release();
        }

        //Function that writes contents to log file

        private static void WriteToLogs(string toLog)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | " + toLog);
            stringBuilder.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = ($"{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}");
            string nowFullPath = Master.systemLogsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";

            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();
        }
    }
}
