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
    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public static class PatchCreateThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (__instance is Mote) return true;
            if (__instance is Projectile) return true;
            if (__instance is Filth) return true;

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit)
            {
                if (OnlineManager.isHost)
                {
                    if (OnlineManager.mapThings.Contains(__instance)) return true;

                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData.activityStepMode = OnlineActivityStepMode.Create;
                    OnlineActivityData.creationOrder = OnlineHelper.CreateCreationOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineHelper.AddToVisitList(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineHelper.ClearQueue();
                        OnlineHelper.AddToVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else
                    {
                        if (__instance is Filth) return false;

                        if (OnlineManager.mapThings.Contains(__instance)) return true;

                        OnlineActivityData OnlineActivityData = new OnlineActivityData();
                        OnlineActivityData.activityStepMode = OnlineActivityStepMode.Create;
                        OnlineActivityData.creationOrder = OnlineHelper.CreateCreationOrder(__instance);

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                        Network.listener.EnqueuePacket(packet);
                        return false;
                    }
                }
            }

            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid)
            {
                if (OnlineManager.isHost)
                {
                    if (OnlineManager.mapThings.Contains(__instance)) return true;

                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData.activityStepMode = OnlineActivityStepMode.Create;
                    OnlineActivityData.creationOrder = OnlineHelper.CreateCreationOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineHelper.AddToVisitList(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineHelper.ClearQueue();
                        OnlineHelper.AddToVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }

            else return true;
        }
    }

    [HarmonyPatch(typeof(Thing), "Destroy")]
    public static class PatchDestroyThing
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;
            if (!OnlineManager.mapThings.Contains(__instance)) return true;

            if (__instance is Mote) return true;
            if (__instance is Projectile) return true;

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit)
            {
                if (OnlineManager.isHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    onlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineHelper.RemoveFromVisitList(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineHelper.ClearQueue();
                        OnlineHelper.RemoveFromVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else
                    {
                        OnlineActivityData onlineActivityData = new OnlineActivityData();
                        onlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                        onlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                        Network.listener.EnqueuePacket(packet);
                        return false;
                    }
                }
            }

            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid)
            {
                if (OnlineManager.isHost)
                {
                    OnlineActivityData onlineActivityData = new OnlineActivityData();
                    onlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    onlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), onlineActivityData);
                    Network.listener.EnqueuePacket(packet);

                    //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                    OnlineHelper.RemoveFromVisitList(__instance);
                    return true;
                }

                else
                {
                    //IF COMING FROM HOST

                    if (OnlineManager.queuedThing == __instance)
                    {
                        OnlineHelper.ClearQueue();
                        OnlineHelper.RemoveFromVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }

            else return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
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
            data.mapTicks = RimworldManager.GetGameTicks();
            data.pawnOrder = OnlineHelper.CreatePawnOrder(___pawn);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);

            //Logger.Warning($"New job! > {newJob.def.defName} > {___pawn.Label}");
        }
    }

    [HarmonyPatch(typeof(Thing), "TakeDamage")]
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
                    onlineActivityData.damageOrder = OnlineHelper.CreateDamageOrder(dinfo, __instance);

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
                    OnlineHelper.ClearQueue();
                    return true;
                }

                //IF PLAYER ASKING FOR

                else
                {
                    __result = new DamageWorker.DamageResult();
                    __result.totalDamageDealt = 0;

                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HediffMaker), "MakeHediff")]
    public static class PatchApplyHediff
    {
        [HarmonyPrefix]
        public static bool DoPre(HediffDef def, Pawn pawn)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            if (OnlineManager.isHost)
            {
                bool shouldCollect = false;
                if (OnlineManager.factionPawns.Contains(pawn)) shouldCollect = true;
                else if (OnlineManager.nonFactionPawns.Contains(pawn)) shouldCollect = true;

                if (shouldCollect) Logger.Warning($"Got hediff '{def.defName}' on {pawn.Label}");
                return true;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(CompSpawnerFilth), "TrySpawnFilth")]
    public static class PatchFilth
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return true;

            return false;
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(Direction8Way), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap1
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit) OnlineManager.StopOnlineActivity();
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid) return;
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return;
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap2
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit) OnlineManager.StopOnlineActivity();
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid) return;
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return;
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndJoinOrCreateCaravan")]
    public static class PatchCaravanExitMap3
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit) OnlineManager.StopOnlineActivity();
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid) return;
            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return;
        }
    }
}
