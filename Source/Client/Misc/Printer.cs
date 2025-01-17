using System;
using GameClient.Values;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Misc
{
    public static class Printer
    {
        //Functions to write logs in different colors

        public static void Message(object value, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(value.ToString(), LogMode.Message, importance); }

        public static void Warning(object value, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(value.ToString(), LogMode.Warning, importance); }

        public static void Error(object value, LogImportanceMode importance = LogImportanceMode.Normal) { WriteToConsole(value.ToString(), LogMode.Error, importance); }

        //Actual function that writes the logs

        private static void WriteToConsole(string text, LogMode mode, LogImportanceMode importance)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (CheckIfShouldPrint(importance))
            {
                string toWrite = $"[RT] > {text}";

                switch (mode)
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
                        throw new Exception($"[RT] > Logger was passed invalid arguments");
                }
            }
        }

        //Checks if the importance of the log has been enabled

        private static bool CheckIfShouldPrint(LogImportanceMode importance)
        {
            if (importance == LogImportanceMode.Normal) return true;
            else if (importance == LogImportanceMode.Verbose && (int)ClientValues.currentVerboseMode >= (int)ClientValues.VerboseMode.Verbose) return true;
            else if (importance == LogImportanceMode.Extreme && (int)ClientValues.currentVerboseMode == (int)ClientValues.VerboseMode.Extreme) return true;
            else return false;
        }
    }
}
