using System;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class Logger
    {
        public static void Message(string message) { WriteToConsole(message, LogMode.Message); }

        public static void Warning(string message) { WriteToConsole(message, LogMode.Warning); }

        public static void Error(string message) { WriteToConsole(message, LogMode.Error); }

        private static void WriteToConsole(string text, LogMode mode = LogMode.Message)
        {
            string toWrite = $"[Rimworld Together] > {text}";

            switch(mode)
            {
                case LogMode.Message:
                    Log.Message(toWrite);
                    break;

                case LogMode.Warning:
                    Log.Warning(toWrite);
                    break;

                case LogMode.Error:
                    Log.Error(toWrite);
                    break;

                default:
                    throw new Exception($"[Rimworld Together] > Logger was passed invalid arguments");
            }
        }
    }
}
