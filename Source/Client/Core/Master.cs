using HugsLib;

namespace RimworldTogether
{
    public class Master : ModBase
    {
        public static Master instance;

        public Master() { instance = this; }

        public override string ModIdentifier => "RimworldTogether";
    }
}
