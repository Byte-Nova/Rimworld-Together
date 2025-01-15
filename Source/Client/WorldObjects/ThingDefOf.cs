using RimWorld;

namespace GameClient.WorldObjects
{
    // Add the EXACT defname of the thing you want to add to the "DefOfs". Make sure to put it in the proper category.
    [DefOf]
    public static class RTFactionDefOf
    {
        public static FactionDef RTNeutral;
        public static FactionDef RTAlly;
        public static FactionDef RTEnemy;
        public static FactionDef RTFaction;
        static RTFactionDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
    }
    [DefOf]
    public static class RTWorldObjectDefOf
    {
        public static WorldObjectDef RTCaravan;

        static RTWorldObjectDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(WorldObjectDefOf));
    }
}
