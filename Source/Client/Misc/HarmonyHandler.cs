using GameClient.Core;
using HarmonyLib;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient.Misc
{
    public static class HarmonyHandler
    {
        private static Harmony Instance { get; set; } = null;

        public static void EnableStartPatches() { new Harmony($"{Master.ModID}-Start").PatchCategory("Start"); }

        public static void EnableMainPatches()
        {
            if (Instance == null) Instance = new Harmony($"{Master.ModID}-Main");
            Instance.PatchAllUncategorized(Assembly.GetExecutingAssembly());
        }

        [OnSessionEnd]
        private static void DisableMainPatches() 
        {
            PatchClassProcessor[] sequence = (from type in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
                where type.HasHarmonyAttribute() select type).Select(CreateClassProcessor).ToArray();

            sequence.DoIf((PatchClassProcessor patchClass) => string.IsNullOrEmpty(patchClass.Category), delegate (PatchClassProcessor patchClass)
            {
                patchClass.Unpatch();
            });
        }

        private static PatchClassProcessor CreateClassProcessor(Type type) { return new PatchClassProcessor(Instance, type); }
    }
}
