using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Core
{
    public static class Master
    {
        public static Map SelectedMap { get; set; } = null;

        public static string ModID { get; private set; } = "RimWorld Together Synchronous";
    }
}
