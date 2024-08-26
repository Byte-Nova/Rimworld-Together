using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RT_SOS2Patches
{
    public static class Main
    {
        private static readonly string patchID = "RT_SOS2Patches";
        static Main() 
        {
            LoadHarmonyPatches();
        }

        public static void LoadHarmonyPatches() 
        {
            Harmony harmony = new Harmony(patchID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

}
