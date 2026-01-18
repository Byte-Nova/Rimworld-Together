using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Defs
{
    [DefOf]
    public static class RTWorldObjectDefOf
    {
        public static WorldObjectDef RTCaravan;

        public static WorldObjectDef RTSettlement;

        public static WorldObjectDef RTSite;

        static RTWorldObjectDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(WorldObjectDefOf));
    }
}
