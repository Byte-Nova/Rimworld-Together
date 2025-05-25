namespace Shared
{
    public class StorytellerValuesFile
    {
        public bool EnforceStoryteller;

        public string StorytellerDefname;

        public override string ToString()
        {
            return $"StorytellerValuesFile:|{EnforceStoryteller}|{StorytellerDefname}";
        }
    }
}