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
                        OnlineHelper.AddToVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else
                    {
                        //if (__instance is Filth) return false;

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
                        OnlineHelper.AddToVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }

            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return true;

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
                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    OnlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
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
                        OnlineHelper.RemoveFromVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else
                    {
                        OnlineActivityData OnlineActivityData = new OnlineActivityData();
                        OnlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                        OnlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

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
                    OnlineActivityData OnlineActivityData = new OnlineActivityData();
                    OnlineActivityData.activityStepMode = OnlineActivityStepMode.Destroy;
                    OnlineActivityData.destructionOrder = OnlineHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), OnlineActivityData);
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
                        OnlineHelper.RemoveFromVisitList(__instance);
                        return true;
                    }

                    //IF PLAYER ASKING FOR

                    else return false;
                }
            }

            else if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return true;

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

            OnlineActivityData visitData = new OnlineActivityData();
            visitData.activityStepMode = OnlineActivityStepMode.Action;
            visitData.mapTicks = RimworldManager.GetGameTicks();
            visitData.pawnOrder = OnlineHelper.CreatePawnOrder(___pawn);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);

            //Logger.Warning($"New job! > {newJob.def.defName} > {___pawn.Label}");
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

            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Visit) return false;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Raid) return false;
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.Misc) return false;
            else return true;
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
