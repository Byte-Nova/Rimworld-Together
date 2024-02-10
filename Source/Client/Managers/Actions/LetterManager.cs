using Verse;

namespace RimworldTogether.GameClient.Managers.Actions
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
