using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class Logger
    {
        //Variables

        private static readonly Semaphore semaphore = new Semaphore(1, 1);

        private static readonly Dictionary<LogMode, ConsoleColor> colorDictionary = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Message, ConsoleColor.White },
            { LogMode.Warning, ConsoleColor.Yellow },
            { LogMode.Error, ConsoleColor.Red },
            { LogMode.Title, ConsoleColor.Green },
            { LogMode.Outsider, ConsoleColor.Magenta}
        };

        //Functions to write logs in different colors

        public static void Message(string message, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(message, LogMode.Message, importance); }

        public static void Warning(string message, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(message, LogMode.Warning, importance); }

        public static void Error(string message, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(message, LogMode.Error, importance); }

        public static void Title(string message, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(message, LogMode.Title, importance); }

        public static void Outsider(string message, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(message, LogMode.Outsider, importance); }

        //Actual function that writes the logs

        private static void WriteToConsole(string text, LogMode mode, LogImportanceMode importance, bool writeToLogs = true)
        {
            semaphore.WaitOne();           

            try
            {
                if (CheckIfShouldPrint(importance))
                {
                    if (writeToLogs) WriteToLogs(text);

                    Console.ForegroundColor = colorDictionary[mode];
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | " + text);
                    Console.ForegroundColor = ConsoleColor.White;

                    if (Master.discordConfig != null && Master.discordConfig.Enabled) DiscordManager.SendMessageToConsoleChannelBuffer(text);
                }
            }
            catch { throw new Exception($"Logger encountered an error. This should never happen"); }

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

        //Checks if the importance of the log has been enabled

        private static bool CheckIfShouldPrint(LogImportanceMode importance)
        {
            if (importance == LogImportanceMode.Normal) return true;
            else if (importance == LogImportanceMode.Verbose && Master.serverConfig.VerboseLogs) return true;
            else if (importance == LogImportanceMode.Extreme && Master.serverConfig.ExtremeVerboseLogs) return true;
            else return false;
        }
    }
}
