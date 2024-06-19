using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;

namespace GameClient
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public static class PatchCreateThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Map map, Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (OnlineManagerHelper.CheckIfIgnoreThingSync(__instance)) return true;

            //Don't execute patch if is different than the online one
            if (OnlineManager.onlineMap != map) return true;
            else
            {
                if (ClientValues.isRealTimeHost)
                {
                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData.activityStepMode = OnlineActivityStepMode.Create;
                    OnlineActivityData.creationOrder = OnlineManagerHelper.CreateCreationOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineManagerHelper.AddThingToMap(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        OnlineManagerHelper.AddThingToMap(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

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
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (OnlineManagerHelper.CheckIfIgnoreThingSync(__instance)) return true;

            //Don't execute patch if map doesn't contain the thing already
            bool shouldPatch = false;
            if (OnlineManager.factionPawns.Contains(__instance)) shouldPatch = true;
            else if (OnlineManager.nonFactionPawns.Contains(__instance)) shouldPatch = true;
            else if (OnlineManager.mapThings.Contains(__instance)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (ClientValues.isRealTimeHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    onlineActivityData.destructionOrder = OnlineManagerHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineManagerHelper.RemoveThingFromMap(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        OnlineManagerHelper.RemoveThingFromMap(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class PatchStartNewJob
    {
        [HarmonyPrefix]
        public static bool DoPre(Job newJob, Pawn ___pawn)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            //Don't execute patch if map doesn't contain the pawn
            bool shouldPatch = false;
            if (OnlineManager.factionPawns.Contains(___pawn)) shouldPatch = true;
            else if (OnlineManager.nonFactionPawns.Contains(___pawn)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (OnlineManager.factionPawns.Contains(___pawn))
                {
                    OnlineActivityData data = new OnlineActivityData();
                    data.activityStepMode = OnlineActivityStepMode.Action;
                    data.pawnOrder = OnlineManagerHelper.CreatePawnOrder(___pawn, newJob);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), data);
                    Network.listener.EnqueuePacket(packet);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == ___pawn)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        return true;
                    }

                    //IF PLAYER ASKING FOR

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
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (!OnlineManager.mapThings.Contains(__instance)) return true;
            else
            {
                if (ClientValues.isRealTimeHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Damage;
                    onlineActivityData.damageOrder = OnlineManagerHelper.CreateDamageOrder(dinfo, __instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        return true;
                    }

                    //IF PLAYER ASKING FOR

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
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            bool shouldPatch = false;
            if (OnlineManager.factionPawns.Contains(___pawn)) shouldPatch = true;
            else if (OnlineManager.nonFactionPawns.Contains(___pawn)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (ClientValues.isRealTimeHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Hediff;
                    onlineActivityData.hediffOrder = OnlineManagerHelper.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Add);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == ___pawn)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        return true;
                    }

                    //IF PLAYER ASKING FOR

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
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            bool shouldPatch = false;
            if (OnlineManager.factionPawns.Contains(___pawn)) shouldPatch = true;
            else if (OnlineManager.nonFactionPawns.Contains(___pawn)) shouldPatch = true;

            if (!shouldPatch) return true;
            else
            {
                if (ClientValues.isRealTimeHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Hediff;
                    onlineActivityData.hediffOrder = OnlineManagerHelper.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Remove);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == ___pawn)
                    {
                        OnlineManagerHelper.ClearThingQueue();
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.Init))]
    public static class PatchAddGameCondition
    {
        [HarmonyPrefix]
        public static bool DoPre(GameCondition __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost)
            {
                OnlineActivityData OnlineActivityData = new OnlineActivityData();
                OnlineActivityData.activityStepMode = OnlineActivityStepMode.GameCondition;
                OnlineActivityData.gameConditionOrder = OnlineManagerHelper.CreateGameConditionOrder(__instance, OnlineActivityApplyMode.Add);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                Network.listener.EnqueuePacket(packet);
                return true;
            }

            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedGameCondition == __instance)
                {
                    OnlineManagerHelper.ClearGameConditionQueue();
                    return true;
                }

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.End))]
    public static class PatchRemoveGameCondition
    {
        [HarmonyPrefix]
        public static bool DoPre(GameCondition __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost)
            {
                OnlineActivityData OnlineActivityData = new OnlineActivityData();
                OnlineActivityData.activityStepMode = OnlineActivityStepMode.GameCondition;
                OnlineActivityData.gameConditionOrder = OnlineManagerHelper.CreateGameConditionOrder(__instance, OnlineActivityApplyMode.Remove);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                Network.listener.EnqueuePacket(packet);
                return true;
            }

            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedGameCondition == __instance)
                {
                    OnlineManagerHelper.ClearGameConditionQueue();
                    return true;
                }

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    [HarmonyPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class PatchTickChanging
    {
        [HarmonyPrefix]
        public static bool DoPre(TickManager __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost)
            {
                if (__instance.CurTimeSpeed > OnlineManager.maximumAllowedTimeSpeed) __instance.CurTimeSpeed = OnlineManager.maximumAllowedTimeSpeed;

                if (OnlineManager.queuedTimeSpeed != (int)__instance.CurTimeSpeed)
                {
                    OnlineManager.queuedTimeSpeed = (int)__instance.CurTimeSpeed;

                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.TimeSpeed;
                    onlineActivityData.timeSpeedOrder = OnlineManagerHelper.CreateTimeSpeedOrder();

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                }
            }

            else
            {
                //Always change the CurTimeSpeed to whatever last update we got from host

                if (__instance.CurTimeSpeed != (TimeSpeed)OnlineManager.queuedTimeSpeed)
                {
                    __instance.CurTimeSpeed = (TimeSpeed)OnlineManager.queuedTimeSpeed;
                }
            }

            return true;
        }
    }

    //Not really needed, used to make the non-host player console not freak out when trying to spawn non-requested things

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class PatchCreatePawn
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost) return true;
            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == __instance) return true;

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    [HarmonyPatch(typeof(Building), nameof(Building.SpawnSetup))]
    public static class PatchCreateBuilding
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost) return true;
            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == __instance) return true;

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    [HarmonyPatch(typeof(Filth), nameof(Filth.SpawnSetup))]
    public static class PatchCreateFilth
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (ClientValues.isRealTimeHost) return true;
            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == __instance) return true;

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    //Patches to make online events finish correctly when getting out of the map

    [HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan), new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(Direction8Way), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap1
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            OnlineManager.RequestStopOnlineActivity();
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan), new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap2
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            OnlineManager.RequestStopOnlineActivity();
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan))]
    public static class PatchCaravanExitMap3
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            OnlineManager.RequestStopOnlineActivity();
        }
    }
}
