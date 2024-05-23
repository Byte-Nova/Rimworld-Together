using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class Logger
    {
        public static Semaphore semaphore = new Semaphore(1, 1);

        public static void Message(string message, bool writeToConsole=true) {
            WriteToLogs(message);
            if(writeToConsole) ConsoleManager.WriteToConsole(message, LogMode.Message); 
        }

        public static void Warning(string message) {
            WriteToLogs(message);
            ConsoleManager.WriteToConsole(message, LogMode.Warning); 
        }

        public static void Error(string message) {
            WriteToLogs(message);
            ConsoleManager.WriteToConsole(message, LogMode.Error); 
        }

        public static void WriteToLogs(string toLog)
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
