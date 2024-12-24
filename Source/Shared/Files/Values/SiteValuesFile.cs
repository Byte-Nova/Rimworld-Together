using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class SiteValuesFile
    {
        public int TimeIntervalMinutes = 30;
        
        public SiteInfoFile[] SiteInfoFiles = new SiteInfoFile[0];
    }
}
