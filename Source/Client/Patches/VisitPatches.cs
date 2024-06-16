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
        [HarmonyPostfix]
        public static void DoPost(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (!ClientValues.isInVisit) return;
            if (!OnlineVisitManager.isHost) return;
            if (__instance is Mote) return;

            CreationOrder creationOrder = new CreationOrder();

            if (DeepScribeHelper.CheckIfThingIsHuman(__instance)) creationOrder.creationType = CreationType.Human;
            else if (DeepScribeHelper.CheckIfThingIsAnimal(__instance)) creationOrder.creationType = CreationType.Animal;
            else creationOrder.creationType = CreationType.Thing;

            if (creationOrder.creationType == CreationType.Human) creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString((Pawn)__instance));
            else if (creationOrder.creationType == CreationType.Animal) creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(AnimalScribeManager.AnimalToString((Pawn)__instance));
            else
            {
                //Modify position based on center cell because RimWorld doesn't store it by default
                __instance.Position = __instance.OccupiedRect().CenterCell;
                creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(ThingScribeManager.ItemToString(__instance, __instance.stackCount));
            }

            OnlineVisitData onlineVisitData = new OnlineVisitData();
            onlineVisitData.visitStepMode = OnlineVisitStepMode.Create;
            onlineVisitData.creationOrder = creationOrder;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
            Network.listener.EnqueuePacket(packet);

            OnlineVisitManager.mapThings.Add(__instance);
            Logger.Warning($"Created! > {OnlineVisitManager.mapThings.IndexOf(__instance)} > {__instance.OccupiedRect().CenterCell}");
        }
    }

    [HarmonyPatch(typeof(Thing), "Destroy")]
    public static class PatchDestroyThingDuringVisit
    {
        [HarmonyPostfix]
        public static void DoPost(Thing __instance)
        {
            if (Network.state == NetworkState.Disconnected) return;
            if (!ClientValues.isInVisit) return;
            if (!OnlineVisitManager.isHost) return;
            if (!OnlineVisitManager.mapThings.Contains(__instance)) return;

            DestructionOrder destructionOrder = new DestructionOrder();
            destructionOrder.indexToDestroy = OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == __instance);

            OnlineVisitData onlineVisitData = new OnlineVisitData();
            onlineVisitData.visitStepMode = OnlineVisitStepMode.Destroy;
            onlineVisitData.destructionOrder = destructionOrder;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), onlineVisitData);
            Network.listener.EnqueuePacket(packet);

            Logger.Warning($"Destroyed! > {OnlineVisitManager.mapThings.IndexOf(__instance)}");
            OnlineVisitManager.mapThings.Remove(__instance);
            return;
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
