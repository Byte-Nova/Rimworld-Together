using HugsLib;

namespace RimworldTogether.GameClient.Core
{
    public class Master : ModBase
    {
        public static Master instance;

        public Master() { instance = this; }

        public override string ModIdentifier => "RimworldTogether";
    }
}
