using HarmonyLib;
using System.Reflection;
using GameClient;
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
            Logger.Message($"Successfuly loaded SOS2Patches");
        }
    }

}
