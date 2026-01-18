using GameClient.Misc;
using HarmonyLib;
using Shared;
using Shared.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Shared.CommonEnumerators;

namespace Synchronous.Core
{
    [StaticConstructorOnStartup]
    public static class Main_
    {
        public static Harmony Instance { get; private set; } = null;

        [OnSynchronousStart]
        private static void EnablePatches()
        {
            if (Instance == null) Instance = new Harmony(Master.ModID);
            Instance.PatchCategory("Synchronous");
            Printer.Warning("Patched Synchronous methods", LogImportanceMode.Verbose);
        }

        [OnSessionEnd]
        [OnSynchronousEnd]
        private static void DisablePatches()
        {
            Instance.UnpatchCategory("Synchronous");
            Printer.Warning("Unpatched Synchronous methods", LogImportanceMode.Verbose);
        }

        public static bool CheckIfShouldPatch(Map map)
        {
            if (SessionHandler.SynchronousMap.Tile != map.Tile) return false;
            else return true;
        }
    }
}
