using GameClient.Core.Configs;
using GameClient.Misc;
using Shared;
using Shared.Misc;
using System;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Hooks.Shared
{
    public static class ClientPrinter
    {
        private static readonly Color RTColor = new Color(140f, 0f, 255f);
        
        public static void CreateLogger()
        {
            Action<object, LogImportanceMode> onMessage = delegate (object value, LogImportanceMode importance)
            {
                if (CheckIfShouldPrint(importance)) MainThreadHandler.Instance.Enqueue(() => { WriteToConsole(value.ToString(), LogMode.Message, importance); });
            };

            Action<object, LogImportanceMode> onWarning = delegate (object value, LogImportanceMode importance)
            {
                if (CheckIfShouldPrint(importance)) MainThreadHandler.Instance.Enqueue(() => { WriteToConsole(value.ToString(), LogMode.Warning, importance); });
            };

            Action<object, LogImportanceMode> onError = delegate (object value, LogImportanceMode importance)
            {
                if (CheckIfShouldPrint(importance)) MainThreadHandler.Instance.Enqueue(() => { WriteToConsole(value.ToString(), LogMode.Error, importance); });
            };

            Printer printer = new Printer(onMessage, onWarning, onError, null);
        }

        private static void WriteToConsole(string text, LogMode mode, LogImportanceMode importance)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (CheckIfShouldPrint(importance))
            {
                string toWrite = $"[RT] > {text}";


                switch (mode)
                {
                    case LogMode.Message:
                        Log.Message(toWrite.Colorize(RTColor));
                        break;

                    case LogMode.Warning:
                        Log.Warning(toWrite.Colorize(RTColor));
                        break;

                    case LogMode.Error:
                        Log.Error(toWrite);
                        break;

                    default:
                        throw new Exception($"[RT] > Logger was passed invalid arguments");
                }
            }
        }

        private static bool CheckIfShouldPrint(LogImportanceMode importance)
        {
            if (importance == LogImportanceMode.Normal) return true;
            else if (importance == LogImportanceMode.Verbose && ModConfigGetter.CurrentVerboseMode >= CommonEnumerators.VerboseMode.Verbose) return true;
            else if (importance == LogImportanceMode.Extreme && ModConfigGetter.CurrentVerboseMode == CommonEnumerators.VerboseMode.Extreme) return true;
            else return false;
        }
    }
}
