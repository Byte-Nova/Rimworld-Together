using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    // Add the EXACT defname of the thing you want to add to the "DefOfs". Make sure to put it in the proper category.
    [DefOf]
    public static class RTSitePartDefOf 
    {
        public static SitePartDef RTFarmland;
        public static SitePartDef RTHunterCamp;
        public static SitePartDef RTQuarry;
        public static SitePartDef RTSawmill;
        public static SitePartDef RTBank;
        public static SitePartDef RTLaboratory;
        public static SitePartDef RTRefinery;
        public static SitePartDef RTHerbalWorkshop;
        public static SitePartDef RTTextileFactory;
        public static SitePartDef RTFoodProcessor;
        static RTSitePartDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
    }
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
