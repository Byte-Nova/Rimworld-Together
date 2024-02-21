using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace RimworldTogether.GameClient.Dialogs
{
    public interface RT_WindowInputs
    {
        public abstract void CacheInputs();

        public abstract void SubstituteInputs(List<object> newInputs);
    }
}
