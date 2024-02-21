using Verse;

namespace GameClient
{
    public static class LetterManager
    {
        public static void GenerateLetter(string title, string description, LetterDef letterType)
        {
            Find.LetterStack.ReceiveLetter(title,
                description,
                letterType);
        }
    }
}
