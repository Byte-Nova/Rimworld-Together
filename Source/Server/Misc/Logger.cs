using System.Text;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class Logger
    {
        public static Semaphore semaphore = new Semaphore(1, 1);

        public static Dictionary<LogMode, ConsoleColor> colorDictionary = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Message, ConsoleColor.White },
            { LogMode.Warning, ConsoleColor.Yellow },
            { LogMode.Error, ConsoleColor.Red },
            { LogMode.Title, ConsoleColor.Green }
        };

        //Variables to help with condensing similar logs to a single log with a multiplier
        private static int repetitionCounter = 1;
        private static string previousText = string.Empty;

        public static void Message(string message) { WriteToConsole(message, LogMode.Message); }

        public static void Warning(string message) { WriteToConsole(message, LogMode.Warning); }

        public static void Error(string message) { WriteToConsole(message, LogMode.Error); }

        public static void WriteToConsole(string text, LogMode mode = LogMode.Message, bool writeToLogs = true, bool allowLogMultiplier = false)
        {
            semaphore.WaitOne();

            Console.CursorVisible = false;

            if (writeToLogs) WriteToLogs(text);

            var (Left, Top) = Console.GetCursorPosition();

            Console.ForegroundColor = colorDictionary[mode];
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);

            //Check if the last log is the same as this log, if so then put a multiplier on the log
            if (text == previousText && allowLogMultiplier)
            {
                repetitionCounter++;

                Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {text} x {repetitionCounter}");
                Console.SetCursorPosition(Left, Top);
            }

            else
            {
                repetitionCounter = 1;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {text}");
                ServerCommandManager.WriteCurrentCommand();
            }

            Console.ForegroundColor = ConsoleColor.White;
            previousText = text;

            Console.CursorVisible = true;
            semaphore.Release();
        }

        public static void ClearCurrentLine()
        {
            semaphore.WaitOne();

            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);

            semaphore.Release();
        }

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
