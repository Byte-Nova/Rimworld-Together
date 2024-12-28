using static Shared.CommonEnumerators;

namespace Shared
{
    public class PawnTargetComponent
    {
        public string[] targets;

        public ActionTargetType[] targetTypes;

        public OnlineActivityTargetFaction[] targetFactions;
    }
}