namespace Shared
{
    using System;
    using System.Linq;

    public static class StringChecker
    {
        private static string[] IllegalSequences { get; } = new string[]
        {
            "<", ">", ":", "\"", "/", "|", "?", "*", "CON", "PRN", "AUX", "NUL", "COM1",
            "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
            "LPT8", "LPT9"
        };

        public static bool CheckIfStringValid(string toCheck)
        {
            if (string.IsNullOrWhiteSpace(toCheck)) return false;
            if (IllegalSequences.Any(s => toCheck.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1 || s.Any(char.IsWhiteSpace))) return false;

            return true;
        }
    }
}