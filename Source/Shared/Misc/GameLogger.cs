using System;

namespace RimworldTogether.Shared.Misc
{
    //To prevent visibility of those inside GameLogger
    public static class LoggerActions
    {
        public static Action<string> LogAction;
        public static Action<string> WarningAction;
        public static Action<string> ErrorAction;
    }

    public static class GameLogger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
            LoggerActions.LogAction?.Invoke(message);
        }

        public static void Warning(string message)
        {
            Console.WriteLine(message);
            LoggerActions.WarningAction?.Invoke(message);
        }

        public static void Error(string message)
        {
            Console.WriteLine(message);
            LoggerActions.ErrorAction?.Invoke(message);
        }
    }
}