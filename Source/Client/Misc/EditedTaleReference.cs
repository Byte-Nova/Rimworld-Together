using RimWorld;
using Verse;

namespace GameClient
{
    public class EditedTaleReference : TaleReference
    {
        public TaggedString editedTale;
        public EditedTaleReference()
        {
            editedTale = new TaggedString("Corrupted");
        }

        public EditedTaleReference(string taleDescription)
        {
            editedTale = new TaggedString(taleDescription);
        }

        public EditedTaleReference(TaggedString taleDescription)
        {
            editedTale = taleDescription;
        }
    }
}
