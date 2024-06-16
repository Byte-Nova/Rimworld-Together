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
    [HarmonyPatch(typeof(CompSpawnerFilth), "TrySpawnFilth")]
    public static class PatchFilthDuringVisit
    {
        [HarmonyPrefix]
        public static bool DoPre()
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (ClientValues.isInVisit) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public static class PatchCreateThingDuringVisit
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (!ClientValues.isInVisit) return true;
            if (__instance is Mote) return true;

            //HOST

            if (OnlineVisitManager.isHost)
            {
                if (OnlineVisitManager.mapThings.Contains(__instance)) return true;

                OnlineVisitData onlineVisitData = new OnlineVisitData();
                onlineVisitData.visitStepMode = OnlineVisitStepMode.Create;
                onlineVisitData.creationOrder = OnlineVisitHelper.CreateCreationOrder(__instance);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
                Network.listener.EnqueuePacket(packet);

                //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                OnlineVisitHelper.AddToVisitList(__instance);
                return true;
            }

            //PLAYER

            else
            {
                //IF COMING FROM HOST

                if (OnlineVisitManager.queuedThing == __instance)
                {
                    OnlineVisitHelper.AddToVisitList(__instance);
                    return true;
                }

                //IF PLAYER ASKING FOR

                else
                {
                    if (__instance is Filth) return false;

                    if (OnlineVisitManager.mapThings.Contains(__instance)) return true;

                    OnlineVisitData onlineVisitData = new OnlineVisitData();
                    onlineVisitData.visitStepMode = OnlineVisitStepMode.Create;
                    onlineVisitData.creationOrder = OnlineVisitHelper.CreateCreationOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
                    Network.listener.EnqueuePacket(packet);
                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "Destroy")]
    public static class PatchDestroyThingDuringVisit
    {
        [HarmonyPrefix]
        public static bool DoPre(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return true;
            if (!ClientValues.isInVisit) return true;
            if (__instance is Mote) return true;

            //HOST

            if (OnlineVisitManager.isHost)
            {
                if (!OnlineVisitManager.mapThings.Contains(__instance)) return true;

                OnlineVisitData onlineVisitData = new OnlineVisitData();
                onlineVisitData.visitStepMode = OnlineVisitStepMode.Destroy;
                onlineVisitData.destructionOrder = OnlineVisitHelper.CreateDestructionOrder(__instance);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
                Network.listener.EnqueuePacket(packet);

                //KEEP ALWAYS AS AT THE BOTTOM AS POSSIBLE
                OnlineVisitHelper.RemoveFromVisitList(__instance);
                return true;
            }

            //PLAYER

            else
            {
                //IF COMING FROM HOST

                if (OnlineVisitManager.queuedThing == __instance)
                {
                    OnlineVisitHelper.RemoveFromVisitList(__instance);
                    return true;
                }

                //IF PLAYER ASKING FOR

                else
                {
                    if (!OnlineVisitManager.mapThings.Contains(__instance)) return true;

                    OnlineVisitData onlineVisitData = new OnlineVisitData();
                    onlineVisitData.visitStepMode = OnlineVisitStepMode.Destroy;
                    onlineVisitData.destructionOrder = OnlineVisitHelper.CreateDestructionOrder(__instance);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
                    Network.listener.EnqueuePacket(packet);
                    return false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public static class PatchGetNewJobFromPawns
    {
        [HarmonyPostfix]
        public static void DoPost(Job newJob, Pawn ___pawn)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (!ClientValues.isInVisit) return;
            if (!OnlineVisitManager.factionPawns.Contains(___pawn)) return;

            OnlineVisitData visitData = new OnlineVisitData();
            visitData.visitStepMode = OnlineVisitStepMode.Action;
            visitData.mapTicks = RimworldManager.GetGameTicks();
            visitData.pawnOrder = OnlineVisitHelper.CreatePawnOrder(___pawn);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);

            //Logger.Warning($"New job! > {newJob.def.defName} > {___pawn.Label}");
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(Direction8Way), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap1
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.isInVisit) OnlineVisitManager.StopVisit();
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan", new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    public static class PatchCaravanExitMap2
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.isInVisit) OnlineVisitManager.StopVisit();
        }
    }

    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndJoinOrCreateCaravan")]
    public static class PatchCaravanExitMap3
    {
        [HarmonyPostfix]
        public static void DoPost()
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (ClientValues.isInVisit) OnlineVisitManager.StopVisit();
        }
    }
}
