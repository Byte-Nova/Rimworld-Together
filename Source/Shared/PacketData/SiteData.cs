using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteData
    {
        public SiteStepMode _stepMode;

        public SiteIdendity _siteFile = new SiteIdendity();
    }
}
