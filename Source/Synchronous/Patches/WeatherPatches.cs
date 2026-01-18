using GameClient.Misc;
using HarmonyLib;
using RimWorld;
using Synchronous.Core;
using Synchronous.Managers;
using Synchronous.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Synchronous.Patches
{
    [HarmonyPatchCategory("Synchronous")]
    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.TransitionTo))]
    public static class P_WeatherManager_TransitionTo
    {
        [HarmonyPrefix]
        public static bool TransitionTo(WeatherDef newWeather, WeatherManager __instance)
        {
            if (PatchHandler.BypassFlag) return true;
            else
            {
                if (!SessionHandler.IsSynchronousHost) return false;
                else if (!Main_.CheckIfShouldPatch(__instance.map)) return true;
                else
                {
                    byte value = (byte)DefDatabase<WeatherDef>.AllDefs.FirstIndexOf(fetch => fetch == newWeather);
                    SWeatherManager.Ask(value);
                    return false;
                }
            }
        }
    }
}
