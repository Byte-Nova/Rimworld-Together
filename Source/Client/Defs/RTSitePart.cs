using GameClient.WorldObjects;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient.Defs
{
    public class RTSitePart : IExposable
    {
        public RTSite site;

        public SitePartDef def;

        public RTSitePart(RTSite site, SitePartDef def)
        {
            this.site = site;
            this.def = def;
        }

        public void ExposeData() { Scribe_Defs.Look(ref def, "def"); }
    }
}
