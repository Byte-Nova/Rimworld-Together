using System.Linq;
using HarmonyLib;
using RimWorld;
using Shared;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class PatchStartNewJob
    {
        [HarmonyPrefix]
        public static bool DoPre(Job newJob, Pawn ___pawn)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(___pawn.Map)) return true;
            else if (!OnlineActivityPatches.CheckIfShouldExecutePatch(___pawn, true, true, false)) return true;
            else
            {
                if (OnlineActivityManager.factionPawns.Contains(___pawn))
                {
                    PawnJobData order = OnlineActivityOrders.CreateJobOrder(___pawn, newJob);
                    OnlineActivityClock.jobOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == ___pawn)
                    {
                        OnlineActivityQueues.SetThingQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public static class PatchCreateThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(__instance.Map)) return true;
            else if (OnlineActivityPatches.CheckIfIgnoreThingSync(__instance)) return true;
            else if (!OnlineActivityPatches.CheckInverseIfShouldPatch(__instance, true, true, true)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    CreationOrderData order = OnlineActivityOrders.CreateCreationOrder(__instance);
                    OnlineActivityClock.creationOrderBuffer.Add(order);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    if (ScriberHelper.CheckIfThingIsHuman(__instance) || ScriberHelper.CheckIfThingIsAnimal(__instance)) 
                    {
                        OnlineActivityManagerHelper.AddPawnToMap((Pawn)__instance);
                    }
                    else OnlineActivityManagerHelper.AddThingToMap(__instance);

                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == __instance)
                    {
                        if (ScriberHelper.CheckIfThingIsHuman(__instance) || ScriberHelper.CheckIfThingIsAnimal(__instance)) 
                        {
                            OnlineActivityManagerHelper.AddPawnToMap((Pawn)__instance);
                        }
                        else OnlineActivityManagerHelper.AddThingToMap(__instance);

                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Destroy))]
    public static class PatchDestroyThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(__instance.Map)) return true;
            else if (OnlineActivityPatches.CheckIfIgnoreThingSync(__instance)) return true;
            else if (!OnlineActivityPatches.CheckIfShouldExecutePatch(__instance, true, true, true)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    DestructionOrderData order = OnlineActivityOrders.CreateDestructionOrder(__instance);
                    OnlineActivityClock.destructionOrderBuffer.Add(order);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    if (ScriberHelper.CheckIfThingIsHuman(__instance) || ScriberHelper.CheckIfThingIsAnimal(__instance)) 
                    {
                        OnlineActivityManagerHelper.RemovePawnFromMap((Pawn)__instance);
                    }
                    else OnlineActivityManagerHelper.RemoveThingFromMap(__instance);

                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == __instance)
                    {
                        if (ScriberHelper.CheckIfThingIsHuman(__instance) || ScriberHelper.CheckIfThingIsAnimal(__instance)) 
                        {
                            OnlineActivityManagerHelper.RemovePawnFromMap((Pawn)__instance);
                        }
                        else OnlineActivityManagerHelper.RemoveThingFromMap(__instance);

                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class PatchApplyDamage
    {
        [HarmonyPrefix]
        public static bool DoPre(DamageInfo dinfo, Thing __instance, ref DamageWorker.DamageResult __result)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(__instance.Map)) return true;
            else if (!OnlineActivityPatches.CheckIfShouldExecutePatch(__instance, false, false, true)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    DamageOrderData order = OnlineActivityOrders.CreateDamageOrder(dinfo, __instance);
                    OnlineActivityClock.damageOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == __instance)
                    {
                        OnlineActivityQueues.SetThingQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else
                    {
                        //Create empty DamageWorker.DamageResult so the functions expecting it don't freak out
                        __result = new DamageWorker.DamageResult();
                        __result.totalDamageDealt = 0f;
                        return false;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult), })]
    public static class PatchAddHediff
    {
        [HarmonyPrefix]
        public static bool DoPre(Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result, Pawn ___pawn)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(___pawn.Map)) return true;
            else if (!OnlineActivityPatches.CheckIfShouldExecutePatch(___pawn, true, true, false)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    HediffOrderData order = OnlineActivityOrders.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Add);
                    OnlineActivityClock.hediffOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == ___pawn)
                    {
                        OnlineActivityQueues.SetThingQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
    public static class PatchRemoveHediff
    {
        [HarmonyPrefix]
        public static bool DoPre(Hediff hediff, Pawn ___pawn)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(___pawn.Map)) return true;
            else if (!OnlineActivityPatches.CheckIfShouldExecutePatch(___pawn, true, true, false)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    HediffOrderData order = OnlineActivityOrders.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Remove);
                    OnlineActivityClock.hediffOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedThing == ___pawn)
                    {
                        OnlineActivityQueues.SetThingQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.RegisterCondition))]
    public static class PatchAddGameCondition
    {
        [HarmonyPrefix]
        public static bool DoPre(GameCondition cond)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(null)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    GameConditionOrderData order = OnlineActivityOrders.CreateGameConditionOrder(cond, OnlineActivityApplyMode.Add);
                    OnlineActivityClock.gameConditionOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedGameCondition == cond)
                    {
                        OnlineActivityQueues.SetGameConditionQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.End))]
    public static class PatchRemoveGameCondition
    {
        [HarmonyPrefix]
        public static bool DoPre(GameCondition __instance)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(null)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    GameConditionOrderData order = OnlineActivityOrders.CreateGameConditionOrder(__instance, OnlineActivityApplyMode.Remove);
                    OnlineActivityClock.gameConditionOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedGameCondition == __instance)
                    {
                        OnlineActivityQueues.SetGameConditionQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }    
        }
    }

    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.TransitionTo))]
    public static class PatchWeatherChange
    {
        [HarmonyPrefix]
        public static bool DoPre(WeatherManager __instance, WeatherDef newWeather)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(__instance.map)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    WeatherOrderData order = OnlineActivityOrders.CreateWeatherOrder(newWeather);
                    OnlineActivityClock.weatherOrderBuffer.Add(order);
                    return true;
                }

                else
                {
                    // IF COMING FROM HOST
                    if (OnlineActivityQueues.queuedWeather == newWeather)
                    {
                        OnlineActivityQueues.SetWeatherQueue(null);
                        return true;
                    }

                    // IF PLAYER ASKING FOR
                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class PatchTickSpeedChange
    {
        [HarmonyPrefix]
        public static bool DoPre(TickManager __instance)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(null)) return true;
            else
            {
                if (SessionValues.isActivityHost)
                {
                    if (OnlineActivityQueues.queuedTimeSpeed != (int)__instance.CurTimeSpeed)
                    {
                        OnlineActivityQueues.queuedTimeSpeed = (int)__instance.CurTimeSpeed;

                        TimeSpeedOrderData order = OnlineActivityOrders.CreateTimeSpeedOrder();
                        OnlineActivityClock.timeSpeedOrderBuffer.Add(order);
                        return true;
                    }
                }

                else
                {
                    // Always change the CurTimeSpeed to whatever last update we got from host

                    if (__instance.CurTimeSpeed != (TimeSpeed)OnlineActivityQueues.queuedTimeSpeed)
                    {
                        __instance.CurTimeSpeed = (TimeSpeed)OnlineActivityQueues.queuedTimeSpeed;
                    }

                    return true;
                }

                return true;
            }
        }
    }

    [HarmonyPatch(typeof(WildAnimalSpawner), nameof(WildAnimalSpawner.SpawnRandomWildAnimalAt))]
    public static class PatchWildAnimalSpawn
    {
        [HarmonyPrefix]
        public static bool DoPre(Map ___map)
        {
            if (!OnlineActivityPatches.CheckIfCanExecutePatch(___map)) return true;
            else
            {
                // Only the host should be allowed to tick for wild animals
                if (SessionValues.isActivityHost) return true;
                else return false;
            }
        }
    }
}
