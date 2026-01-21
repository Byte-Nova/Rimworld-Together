using GameClient.Core;
using HarmonyLib;
using Shared;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GameClient.Misc
{
    public static class HarmonyHandler
    {
        private static Harmony Instance { get; set; } = null;

        private static readonly string HarmonyStartID = $"{Master.ModID}-Start";

        private static readonly string HarmonyMainID = $"{Master.ModID}-Main";

        public static void EnableStartPatches() { new Harmony(HarmonyStartID).PatchCategory("Start"); }

        public static void EnableMainPatches()
        {
            if (Instance == null) Instance = new Harmony(HarmonyMainID);
            Instance.PatchAllUncategorized(Assembly.GetExecutingAssembly());

            CheckForModCollision();
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

        private static void CheckForModCollision()
        {
            List<string> CollidingMods = new List<string>();

            foreach (MethodBase method in Instance.GetPatchedMethods())
            {
                HarmonyLib.Patches patchInfo = Harmony.GetPatchInfo(method);

                foreach (HarmonyLib.Patch patch in patchInfo.Prefixes)
                {
                    if (patch.owner != HarmonyMainID)
                    {
                        if (!CollidingMods.Contains(patch.owner)) CollidingMods.Add(patch.owner);
                    }
                }

                foreach (HarmonyLib.Patch patch in patchInfo.Transpilers)
                {
                    if (patch.owner != HarmonyMainID)
                    {
                        if (!CollidingMods.Contains(patch.owner)) CollidingMods.Add(patch.owner);
                    }
                }
            }

            foreach (string str in CollidingMods)
            {
                if (str == HarmonyStartID) continue;
                else Printer.Warning($"Mod '{str}' is colliding with RimWorld Together! This may cause issues!");
            }
        }
    }
}
