namespace Shared
{
    public static class StringChecker
    {
        private static string[] IllegalChars { get; } = new string[]
        {
            "<", ">", ":", "\"", "/", "|", "?", "*", "CON", "PRN", "AUX", "NUL", "COM1",
            "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
            "LPT8", "LPT9", " "
        };

        public static bool CheckIfStringValid(string toCheck)
        {
            if (string.IsNullOrEmpty(toCheck)) return false;
            if (string.IsNullOrWhiteSpace(toCheck)) return false;
            foreach (string str in IllegalChars)
            {
                if (toCheck.Contains(str)) return false;
            }
            
            return true;
        }
    }
}