using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace RimworldTogether.GameClient.Dialogs
{
    interface RT_Window
    {

        public abstract List<Object> GetInputList();
    }
}
