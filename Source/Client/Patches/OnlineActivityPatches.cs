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
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (OnlineManagerHelper.CheckIfIgnoreThingSync(__instance)) return true;

            if (OnlineManager.isHost)
            {
                if (OnlineManager.mapThings.Contains(__instance)) return true;

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

                else
                {
                    return false;

                    //OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    //OnlineActivityData.activityStepMode = OnlineActivityStepMode.Create;
                    //OnlineActivityData.creationOrder = OnlineManagerHelper.CreateCreationOrder(__instance);

                    //Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                    //Network.listener.EnqueuePacket(packet);
                    //return false;
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

            //TODO
            //CHANGE THE WAY WE CHECK IF IT'S TARGETING THE ACTIVITY MAP
            if (!OnlineManager.mapThings.Contains(__instance)) return true;

            if (OnlineManager.isHost)
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

                else
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    onlineActivityData.destructionOrder = OnlineManagerHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class PatchStartNewJob
    {
        [HarmonyPostfix]
        public static void DoPost(Job newJob, Pawn ___pawn)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;
            if (!OnlineManager.factionPawns.Contains(___pawn)) return;

            OnlineActivityData data = new OnlineActivityData();
            data.activityStepMode = OnlineActivityStepMode.Action;
            data.pawnOrder = OnlineManagerHelper.CreatePawnOrder(___pawn);
            data.timeSpeedOrder = OnlineManagerHelper.CreateTimeSpeedOrder();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);
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

            if (OnlineManager.isHost)
            {
                bool shouldCollect = false;
                if (OnlineManager.factionPawns.Contains(__instance)) shouldCollect = true;
                else if (OnlineManager.nonFactionPawns.Contains(__instance)) shouldCollect = true;
                else if (OnlineManager.mapThings.Contains(__instance)) shouldCollect = true;

                if (shouldCollect)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Damage;
                    onlineActivityData.damageOrder = OnlineManagerHelper.CreateDamageOrder(dinfo, __instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);
                }

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
                    __result = new DamageWorker.DamageResult();
                    __result.totalDamageDealt = 0f;
                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult), })]
    public static class PatchApplyHediff
    {
        [HarmonyPrefix]
        public static bool DoPre(Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result, Pawn ___pawn)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (OnlineManager.isHost)
            {
                bool shouldCollect = false;
                if (OnlineManager.factionPawns.Contains(___pawn)) shouldCollect = true;
                else if (OnlineManager.nonFactionPawns.Contains(___pawn)) shouldCollect = true;

                if (shouldCollect)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Hediff;
                    onlineActivityData.hediffOrder = OnlineManagerHelper.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Add);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    if (hediff.Part != null) Logger.Warning($"Got hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity at body part '{hediff.Part.def.defName}'");
                    else Logger.Warning($"Got hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity'");
                }

                return true;
            }
            
            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == ___pawn)
                {
                    OnlineManagerHelper.ClearThingQueue();

                    if (hediff.Part != null) Logger.Warning($"Set hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity at body part '{hediff.Part.def.defName}'");
                    else Logger.Warning($"Set hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity'");
                    return true;
                }

                //IF PLAYER ASKING FOR

                else return false;
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

            if (OnlineManager.isHost)
            {
                bool shouldCollect = false;
                if (OnlineManager.factionPawns.Contains(___pawn)) shouldCollect = true;
                else if (OnlineManager.nonFactionPawns.Contains(___pawn)) shouldCollect = true;

                if (shouldCollect)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Hediff;
                    onlineActivityData.hediffOrder = OnlineManagerHelper.CreateHediffOrder(hediff, ___pawn, OnlineActivityApplyMode.Remove);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    if (hediff.Part != null) Logger.Warning($"Deleted hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity at body part '{hediff.Part.def.defName}'");
                    else Logger.Warning($"Deleted hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity'");
                }

                return true;
            }

            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == ___pawn)
                {
                    OnlineManagerHelper.ClearThingQueue();

                    if (hediff.Part != null) Logger.Warning($"Deleted hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity at body part '{hediff.Part.def.defName}'");
                    else Logger.Warning($"Deleted hediff '{hediff.def.defName}' on {___pawn.Label} with '{hediff.Severity}' severity'");
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

            if (OnlineManager.isHost)
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

    //Not really needed, used to make the non-host player console not freak out when trying to spawn non-requested filth

    [HarmonyPatch(typeof(Filth), nameof(Filth.SpawnSetup))]
    public static class PatchCreateFilth
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (OnlineManager.isHost) return true;
            else
            {
                //IF COMING FROM HOST

                if (OnlineManager.queuedThing == __instance) return true;

                //IF PLAYER ASKING FOR

                else return false;
            }
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan), new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(Direction8Way), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap1
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            OnlineManager.StopOnlineActivity();
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

            OnlineManager.StopOnlineActivity();
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

            OnlineManager.StopOnlineActivity();
        }
    }
}
