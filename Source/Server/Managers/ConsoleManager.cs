using System.Threading;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ConsoleManager
    {
        public static Semaphore semaphore = new Semaphore(1, 1);

        //history of commands and the current one being written
        //Index 0 is the current command being written
        public static List<string> commandHistory = new() { "" };
        public static int commandHistoryPosition = 0;

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

        public static void ListenForServerCommands()
        {
            List<string> tabbedCommands = new List<string>();
            int tabbedCommandsIndex = 0;

            while (true)
            {

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ConsoleKeyInfo cki = Console.ReadKey(true);

                switch (cki.Key)
                {
                    case ConsoleKey.Enter:

                        if (commandHistoryPosition != 0) commandHistory[0] = commandHistory[commandHistoryPosition];
                        if (commandHistory.Count() >= 20) commandHistory.RemoveAt(commandHistory.Count() - 1);

                        ClearCurrentLine();
                        Logger.Message(commandHistory[0], writeToConsole:false);

                        commandHistory.Insert(0, "");
                        commandHistoryPosition = 0;

                        ServerCommandManager.ParseServerCommands(commandHistory[1]);
                        continue;

                    case ConsoleKey.Backspace:
                        if (commandHistory[0].Count() > 0) commandHistory[0] = commandHistory[0].Substring(0, commandHistory[0].Count() - 1);
                        break;

                    case ConsoleKey.UpArrow:
                        if (commandHistoryPosition != commandHistory.Count() - 1) commandHistoryPosition++;
                        break;

                    case ConsoleKey.DownArrow:
                        if (commandHistoryPosition != 0) commandHistoryPosition--;
                        break;

                    case ConsoleKey.Tab:
                        if (tabbedCommands.Count() > 0)
                        {
                            tabbedCommandsIndex++;
                            if (tabbedCommandsIndex >= tabbedCommands.Count())
                            {
                                tabbedCommandsIndex = 0;
                                commandHistory[0] = tabbedCommands[tabbedCommandsIndex];
                            }
                        }

                        else
                        {
                            tabbedCommands = ServerCommandManager.commandDictionary.Keys.ToList().FindAll(x => x.StartsWith(commandHistory[0], StringComparison.OrdinalIgnoreCase)).ToList();
                            if (tabbedCommands.Count() > 0) commandHistory[0] = tabbedCommands[0];
                        }
                        break;

                    default:
                        commandHistory[0] += cki.KeyChar;
                        break;
                }

                if (cki.Key != ConsoleKey.Tab)
                {
                    tabbedCommands.Clear();
                    tabbedCommandsIndex = -1;
                }

                Console.CursorVisible = false;

                ClearCurrentLine();
                Console.Write($"{commandHistory[commandHistoryPosition]}");

                Console.CursorVisible = true;

            }
        }
        public static void WriteCurrentCommand()
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(commandHistory[0]);
            Console.ForegroundColor = currentColor;
        }

        public static void WriteToConsole(string text, LogMode mode = LogMode.Message, bool writeToLogs = true, bool allowLogMultiplier = false, bool broadcast = true)
        {
            semaphore.WaitOne();

            Console.CursorVisible = false;

            if (writeToLogs) Logger.WriteToLogs(text);
            if (broadcast && Master.serverConfig!=null && Master.serverConfig.BroadcastConsoleToAdmins) ChatManager.BroadcastConsoleMessage(text);

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
                ConsoleManager.WriteCurrentCommand();
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
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);

            semaphore.Release();
        }
    }
}
