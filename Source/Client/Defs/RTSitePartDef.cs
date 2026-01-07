using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Defs
{
    public class RTSitesModExtension : DefModExtension
    {
        
    }

    [StaticConstructorOnStartup]
    public class RTSitePartDef
    {
        public static List<SitePartDef> sites;

        public RTSitesModExtension extension;

        static RTSitePartDef()
        {
            sites = DefDatabase<SitePartDef>.AllDefsListForReading.Where(fetch => fetch.HasModExtension<RTSitesModExtension>()).ToList<SitePartDef>();
        }
    }
}