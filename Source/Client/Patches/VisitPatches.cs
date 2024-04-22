using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

namespace GameClient
{
    [HarmonyPatch(typeof(CompSpawnerFilth), "TrySpawnFilth")]
    public static class PatchFilthDuringVisit
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (!Network.isConnectedToServer) return true;
            else
            {
                if (ClientValues.isInVisit) return false;
                else return true;
            }
        }
    }

    [HarmonyPatch(typeof(Game), "LoadGame")]
    public static class SeedGameLoad
    {
        public static bool Prefix()
        {
            if (!Network.isConnectedToServer) return true;

            Rand.PushState();

            Rand.Seed = 1;

            return true;
        }

        public static void Postfix()
        {
            Rand.PopState();

            Log.Warning("Patched");
        }
    }

    [HarmonyPatch(typeof(Map), "FinalizeLoading")]
    public static class SeedMapFinalizeLoading
    {
        public static bool Prefix(Map __instance, ref bool __state)
        {
            if (!Network.isConnectedToServer) return true;

            int seed = __instance.uniqueID;

            Rand.PushState(seed);

            __state = true;

            return true;
        }

        public static void Postfix(Map __instance, bool __state)
        {
            if (__state) Rand.PopState();

            Log.Warning("Patched");
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), nameof(CaravanEnterMapUtility.Enter), new[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool) })]
    static class SeedCaravanEnter
    {
        public static bool Prefix(Map map, ref bool __state)
        {
            if (!Network.isConnectedToServer) return true;

            int seed = map.uniqueID;

            Rand.PushState(seed);

            __state = true;

            return true;
        }

        static void Postfix(Map map, bool __state)
        {
            if (__state) Rand.PopState();

            Log.Warning("Patched");
        }
    }
}
