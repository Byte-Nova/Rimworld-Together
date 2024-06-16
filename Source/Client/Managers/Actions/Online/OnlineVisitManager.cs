using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using static Shared.CommonEnumerators;
using static UnityEngine.GraphicsBuffer;


namespace GameClient
{
    public static class OnlineVisitManager
    {
        public static List<Pawn> factionPawns = new List<Pawn>();
        public static List<Pawn> nonFactionPawns = new List<Pawn>();
        public static List<Thing> mapThings = new List<Thing>();
        public static Map visitMap;

        public static bool isHost;
        public static readonly int tickTime = 1000;

        public static void ParseVisitPacket(Packet packet)
        {
            OnlineVisitData visitData = (OnlineVisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (visitData.visitStepMode)
            {
                case OnlineVisitStepMode.Request:
                    OnVisitRequest(visitData);
                    break;

                case OnlineVisitStepMode.Accept:
                    OnVisitAccept(visitData);
                    break;

                case OnlineVisitStepMode.Reject:
                    OnVisitReject();
                    break;

                case OnlineVisitStepMode.Unavailable:
                    OnVisitUnavailable();
                    break;

                case OnlineVisitStepMode.Action:
                    OnlineVisitHelper.ReceivePawnOrder(visitData);
                    break;

                case OnlineVisitStepMode.Create:
                    OnlineVisitHelper.ReceiveCreationOrder(visitData);
                    break;

                case OnlineVisitStepMode.Destroy:
                    OnlineVisitHelper.ReceiveDestructionOrder(visitData);
                    break;

                case OnlineVisitStepMode.Stop:
                    OnVisitStop();
                    break;
            }
        }

        public static void RequestVisit()
        {
            if (ClientValues.isInVisit) DialogManager.PushNewDialog(new RT_Dialog_Error("You are already visiting someone!"));
            else
            {
                Action r1 = delegate
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for visit response"));

                    OnlineVisitData visitData = new OnlineVisitData();
                    visitData.visitStepMode = (int)OnlineVisitStepMode.Request;
                    visitData.fromTile = Find.AnyPlayerHomeMap.Tile;
                    visitData.targetTile = ClientValues.chosenSettlement.Tile;
                    visitData.caravanHumans = OnlineVisitHelper.GetHumansForVisit(FetchMode.Player);
                    visitData.caravanAnimals = OnlineVisitHelper.GetAnimalsForVisit(FetchMode.Player);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    Network.listener.dataQueue.Enqueue(packet);
                };

                var d1 = new RT_Dialog_YesNo("This feature is still in beta, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
        }

        private static void VisitMap(MapData mapData, OnlineVisitData visitData)
        {
            isHost = false;

            visitMap = MapScribeManager.StringToMap(mapData, true, true, false, true, false, true);
            factionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Player, null);
            mapThings = RimworldManager.GetThingsInMap(visitMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

            OnlineVisitHelper.SpawnPawnsForVisit(FetchMode.Player, visitData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            CameraJumper.TryJump(IntVec3.Zero, visitMap);

            ClientValues.ToggleVisit(true);
            RimworldManager.SetGameTicks(visitData.mapTicks);
        }

        public static void StopVisit()
        {
            OnlineVisitData visitData = new OnlineVisitData();
            visitData.visitStepMode = OnlineVisitStepMode.Stop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }

        private static void OnVisitRequest(OnlineVisitData visitData)
        {
            Action r1 = delegate
            {
                isHost = true;
                visitMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == visitData.targetTile).Map;
                factionPawns = OnlineVisitHelper.GetMapPawns(FetchMode.Host, null);
                mapThings = RimworldManager.GetThingsInMap(visitMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

                SendRequestedMap(visitData);
                OnlineVisitHelper.SpawnPawnsForVisit(FetchMode.Host, visitData);
                ClientValues.ToggleVisit(true);
            };

            Action r2 = delegate
            {
                visitData.visitStepMode = OnlineVisitStepMode.Reject;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                Network.listener.dataQueue.Enqueue(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Visited by {visitData.visitorName}, accept?", r1, r2);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnVisitAccept(OnlineVisitData visitData)
        {
            DialogManager.PopWaitDialog();

            MapData mapData = (MapData)Serializer.ConvertBytesToObject(visitData.mapDetails);

            Action r1 = delegate { VisitMap(mapData, visitData); };
            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Map received but contains unknown mod data", r1));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_OK("Map received", r1));
        }

        private static void OnVisitReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the visit!"));
        }

        private static void OnVisitUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must be online!"));
        }

        private static void OnVisitStop()
        {
            if (!ClientValues.isInVisit) return;
            else
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Visiting event ended"));

                foreach (Pawn pawn in nonFactionPawns.ToArray()) pawn.Destroy();

                ClientValues.ToggleVisit(false);
            }
        }

        private static void SendRequestedMap(OnlineVisitData visitData)
        {
            visitData.visitStepMode = OnlineVisitStepMode.Accept;
            visitData.mapHumans = OnlineVisitHelper.GetHumansForVisit(FetchMode.Host);
            visitData.mapAnimals = OnlineVisitHelper.GetAnimalsForVisit(FetchMode.Host);
            visitData.mapTicks = RimworldManager.GetGameTicks();

            MapData mapData = MapManager.ParseMap(visitMap, true, true, true, true);
            visitData.mapDetails = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }
    }

    public static class OnlineVisitHelper
    {
        public static PawnOrder CreatePawnOrder(Pawn pawn)
        {
            PawnOrder pawnOrder = new PawnOrder();
            pawnOrder.pawnIndex = OnlineVisitManager.factionPawns.IndexOf(pawn);

            pawnOrder.defName = pawn.jobs.curJob.def.defName;
            pawnOrder.actionTargets = GetActionTargets(pawn.jobs.curJob);
            pawnOrder.actionIndexes = GetActionIndexes(pawn.jobs.curJob, pawnOrder);
            pawnOrder.actionTypes = GetActionTypes(pawn.jobs.curJob);

            pawnOrder.queueTargetsA = GetQueuedActionTargets(pawn.jobs.curJob, 0);
            pawnOrder.queueIndexesA = GetQueuedActionIndexes(pawn.jobs.curJob, 0);
            pawnOrder.queueTypesA = GetQueuedActionTypes(pawn.jobs.curJob, 0);

            pawnOrder.queueTargetsB = GetQueuedActionTargets(pawn.jobs.curJob, 1);
            pawnOrder.queueIndexesB = GetQueuedActionIndexes(pawn.jobs.curJob, 1);
            pawnOrder.queueTypesB = GetQueuedActionTypes(pawn.jobs.curJob, 1);

            pawnOrder.isDrafted = GetPawnDraftState(pawn);
            pawnOrder.positionSync = ValueParser.Vector3ToString(pawn.Position);
            pawnOrder.rotationSync = ValueParser.Rot4ToInt(pawn.Rotation);

            return pawnOrder;
        }

        public static void ReceivePawnOrder(OnlineVisitData visitData)
        {
            if (!ClientValues.isInVisit) return;
            if (!OnlineVisitManager.isHost) RimworldManager.SetGameTicks(visitData.mapTicks);

            List<Pawn> otherPawns = GetOtherFactionPawnsSecure();
            Pawn pawn = otherPawns[visitData.pawnOrder.pawnIndex];

            IntVec3 jobPositionStart = ValueParser.StringToVector3(visitData.pawnOrder.positionSync);
            Rot4 jobRotationStart = ValueParser.StringToRot4(visitData.pawnOrder.rotationSync);
            ChangePawnTransform(pawn, jobPositionStart, jobRotationStart);
            HandlePawnDrafting(pawn, visitData.pawnOrder.isDrafted);

            try
            {
                JobDef jobDef = RimworldManager.GetJobFromDef(visitData.pawnOrder.defName);
                LocalTargetInfo targetA = GetActionTargetsFromString(visitData.pawnOrder, 0);
                LocalTargetInfo targetB = GetActionTargetsFromString(visitData.pawnOrder, 1);
                LocalTargetInfo targetC = GetActionTargetsFromString(visitData.pawnOrder, 2);
                LocalTargetInfo[] targetQueueA = GetQueuedActionTargetsFromString(visitData.pawnOrder, 0);
                LocalTargetInfo[] targetQueueB = GetQueuedActionTargetsFromString(visitData.pawnOrder, 1);

                Job newJob = RimworldManager.SetJobFromDef(jobDef, targetA, targetB, targetC);
                newJob.count = visitData.pawnOrder.count;

                foreach (LocalTargetInfo target in targetQueueA) newJob.AddQueuedTarget(TargetIndex.A, target);
                foreach (LocalTargetInfo target in targetQueueB) newJob.AddQueuedTarget(TargetIndex.B, target);

                ChangeCurrentJob(pawn, newJob);
                ChangeJobSpeedIfNeeded(newJob);
            }
            catch { Logger.Warning($"Couldn't set job for human {pawn.Name}"); }
        }

        public static void ReceiveCreationOrder(OnlineVisitData visitData)
        {
            Thing toSpawn;

            if (visitData.creationOrder.creationType == CreationType.Human)
            {
                HumanData data = (HumanData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                toSpawn = HumanScribeManager.StringToHuman(data);
            }

            else if (visitData.creationOrder.creationType == CreationType.Animal)
            {
                AnimalData data = (AnimalData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                toSpawn = AnimalScribeManager.StringToAnimal(data);
            }

            else
            {
                ItemData data = (ItemData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                toSpawn = ThingScribeManager.StringToItem(data);
            }

            RimworldManager.PlaceThingInMap(toSpawn, OnlineVisitManager.visitMap);
            AddToVisitList(toSpawn);
        }

        public static void ReceiveDestructionOrder(OnlineVisitData visitData)
        {
            Thing toDestroy = OnlineVisitManager.mapThings[visitData.destructionOrder.indexToDestroy];
            toDestroy.Destroy(DestroyMode.Vanish);
            RemoveFromVisitList(toDestroy);
        }

        public enum VisitListType { Pawn, Things }

        public static void AddToVisitList(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) OnlineVisitManager.nonFactionPawns.Add((Pawn)thing);
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) OnlineVisitManager.nonFactionPawns.Add((Pawn)thing);
            else OnlineVisitManager.mapThings.Add(thing);

            Logger.Warning($"Created! > {thing.def.defName}");
        }

        public static void RemoveFromVisitList(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) OnlineVisitManager.nonFactionPawns.Remove((Pawn)thing);
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) OnlineVisitManager.nonFactionPawns.Remove((Pawn)thing);
            else OnlineVisitManager.mapThings.Remove(thing);

            Logger.Warning($"Destroyed! > {thing.def.defName}");
        }

        public static LocalTargetInfo GetActionTargetsFromString(PawnOrder pawnOrder, int index)
        {
            LocalTargetInfo toGet = LocalTargetInfo.Invalid;

            try
            {
                switch (pawnOrder.actionTypes[index])
                {
                    case ActionTargetType.Thing:
                        toGet = new LocalTargetInfo(OnlineVisitManager.mapThings[pawnOrder.actionIndexes[index]]);
                        break;

                    case ActionTargetType.Human:
                        toGet = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[pawnOrder.actionIndexes[index]]);
                        break;

                    case ActionTargetType.Animal:
                        toGet = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[pawnOrder.actionIndexes[index]]);
                        break;

                    case ActionTargetType.Cell:
                        toGet = new LocalTargetInfo(ValueParser.StringToVector3(pawnOrder.actionTargets[index]));
                        break;
                }
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            return toGet;
        }

        public static LocalTargetInfo[] GetQueuedActionTargetsFromString(PawnOrder pawnOrder, int index)
        {
            List<LocalTargetInfo> toGet = new List<LocalTargetInfo>();

            int[] actionTargetIndexes = null;
            string[] actionTargets = null;
            ActionTargetType[] actionTargetTypes = null;

            if (index == 0)
            {
                actionTargetIndexes = pawnOrder.queueIndexesA.ToArray();
                actionTargets = pawnOrder.queueTargetsA.ToArray();
                actionTargetTypes = pawnOrder.queueTypesA.ToArray();
            }

            else if (index == 1)
            {
                actionTargetIndexes = pawnOrder.queueIndexesB.ToArray();
                actionTargets = pawnOrder.queueTargetsB.ToArray();
                actionTargetTypes = pawnOrder.queueTypesB.ToArray();
            }

            for(int i = 0; i < actionTargets.Length; i++)
            {
                try
                {
                    switch (actionTargetTypes[index])
                    {
                        case ActionTargetType.Thing:
                            toGet.Add(new LocalTargetInfo(OnlineVisitManager.mapThings[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Human:
                            toGet.Add(new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Animal:
                            toGet.Add(new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Cell:
                            toGet.Add(new LocalTargetInfo(ValueParser.StringToVector3(actionTargets[i])));
                            break;
                    }
                }
                catch (Exception e) { Logger.Error(e.ToString()); }
            }

            return toGet.ToArray();
        }

        public static string[] GetActionTargets(Job job)
        {
            List<string> targetInfoList = new List<string>();

            for(int i = 0; i < 3; i++)
            {
                LocalTargetInfo target = null;
                if (i == 0) target = job.targetA;
                else if (i == 1) target = job.targetB;
                else if (i == 2) target = job.targetC;

                try
                {
                    if (target.Thing == null) targetInfoList.Add(ValueParser.Vector3ToString(target.Cell));
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(target.Thing)) targetInfoList.Add(Serializer.SerializeToString(HumanScribeManager.HumanToString(target.Pawn)));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(target.Thing)) targetInfoList.Add(Serializer.SerializeToString(AnimalScribeManager.AnimalToString(target.Pawn)));
                        else targetInfoList.Add(Serializer.SerializeToString(ThingScribeManager.ItemToString(target.Thing, target.Thing.stackCount)));
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetInfoList.ToArray();
        }

        public static int[] GetActionIndexes(Job job, PawnOrder pawnOrder)
        {
            List<int> targetIndexList = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                LocalTargetInfo target = null;
                if (i == 0) target = job.targetA;
                else if (i == 1) target = job.targetB;
                else if (i == 2) target = job.targetC;

                try
                {
                    if (target.Thing == null) targetIndexList.Add(0);
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(target.Thing)) targetIndexList.Add(OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == target.Thing));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(target.Thing)) targetIndexList.Add(OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == target.Thing));
                        else
                        {
                            pawnOrder.count = OnlineVisitManager.mapThings.Find(fetch => fetch == target.Thing).stackCount;
                            targetIndexList.Add(OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == target.Thing));
                        }
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetIndexList.ToArray();
        }

        public static ActionTargetType[] GetActionTypes(Job job)
        {
            List<ActionTargetType> targetTypeList = new List<ActionTargetType>();

            for (int i = 0; i < 3; i++)
            {
                LocalTargetInfo target = null;
                if (i == 0) target = job.targetA;
                else if (i == 1) target = job.targetB;
                else if (i == 2) target = job.targetC;

                try
                {
                    if (target.Thing == null) targetTypeList.Add(ActionTargetType.Cell);
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(target.Thing)) targetTypeList.Add(ActionTargetType.Human);
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(target.Thing)) targetTypeList.Add(ActionTargetType.Animal);
                        else targetTypeList.Add(ActionTargetType.Thing);
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetTypeList.ToArray();
        }

        public static string[] GetQueuedActionTargets(Job job, int index)
        {
            List<string> targetInfoList = new List<string>();

            List<LocalTargetInfo> selectedQueue = new List<LocalTargetInfo>();
            if (index == 0) selectedQueue = job.targetQueueA;
            else if (index == 1) selectedQueue = job.targetQueueB;

            if (selectedQueue == null) return targetInfoList.ToArray();
            for (int i = 0; i < selectedQueue.Count; i++)
            {
                try
                {
                    if (selectedQueue[i].Thing == null) targetInfoList.Add(ValueParser.Vector3ToString(selectedQueue[i].Cell));
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(selectedQueue[i].Thing)) targetInfoList.Add(Serializer.SerializeToString(HumanScribeManager.HumanToString(selectedQueue[i].Pawn)));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(selectedQueue[i].Thing)) targetInfoList.Add(Serializer.SerializeToString(AnimalScribeManager.AnimalToString(selectedQueue[i].Pawn)));
                        else targetInfoList.Add(Serializer.SerializeToString(ThingScribeManager.ItemToString(selectedQueue[i].Thing, 1)));
                    }
                }
                catch { Logger.Error($"failed to parse {selectedQueue[i]}"); }
            }

            return targetInfoList.ToArray();
        }

        public static int[] GetQueuedActionIndexes(Job job, int index)
        {
            List<int> targetIndexList = new List<int>();

            List<LocalTargetInfo> selectedQueue = new List<LocalTargetInfo>();
            if (index == 0) selectedQueue = job.targetQueueA;
            else if (index == 1) selectedQueue = job.targetQueueB;

            if (selectedQueue == null) return targetIndexList.ToArray();
            for (int i = 0; i < selectedQueue.Count; i++)
            {
                try
                {
                    if (selectedQueue[i].Thing == null) targetIndexList.Add(0);
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(selectedQueue[i].Thing)) targetIndexList.Add(OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(selectedQueue[i].Thing)) targetIndexList.Add(OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
                        else targetIndexList.Add(OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
                    }
                }
                catch { Logger.Error($"failed to parse {selectedQueue[i]}"); }
            }

            return targetIndexList.ToArray();
        }

        public static ActionTargetType[] GetQueuedActionTypes(Job job, int index)
        {
            List<ActionTargetType> targetTypeList = new List<ActionTargetType>();

            List<LocalTargetInfo> selectedQueue = new List<LocalTargetInfo>();
            if (index == 0) selectedQueue = job.targetQueueA;
            else if (index == 1) selectedQueue = job.targetQueueB;

            if (selectedQueue == null) return targetTypeList.ToArray();
            for (int i = 0; i < selectedQueue.Count; i++)
            {
                try
                {
                    if (selectedQueue[i].Thing == null) targetTypeList.Add(ActionTargetType.Cell);
                    else
                    {
                        if (DeepScribeHelper.CheckIfThingIsHuman(selectedQueue[i].Thing)) targetTypeList.Add(ActionTargetType.Human);
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(selectedQueue[i].Thing)) targetTypeList.Add(ActionTargetType.Animal);
                        else targetTypeList.Add(ActionTargetType.Thing);
                    }
                }
                catch { Logger.Error($"failed to parse {selectedQueue[i]}"); }
            }

            return targetTypeList.ToArray();
        }

        public static void HandlePawnDrafting(Pawn pawn, bool shouldBeDrafted)
        {
            try
            {
                pawn.drafter ??= new Pawn_DraftController(pawn);

                if (shouldBeDrafted) pawn.drafter.Drafted = true;
                else { pawn.drafter.Drafted = false; }
            }
            catch (Exception e) { Logger.Warning(e.ToString()); }
        }

        public static void ChangePawnTransform(Pawn pawn, IntVec3 pawnPosition, Rot4 pawnRotation)
        {
            pawn.Position = pawnPosition;
            pawn.Rotation = pawnRotation;
            pawn.pather.Notify_Teleported_Int();
        }

        public static void ChangeCurrentJob(Pawn pawn, Job newJob)
        {
            pawn.jobs.ClearQueuedJobs();
            if (pawn.jobs.curJob != null) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);

            pawn.Reserve(newJob.targetA, newJob);
            newJob.TryMakePreToilReservations(pawn, false);
            pawn.jobs.StartJob(newJob);
        }

        public static void ChangeJobSpeedIfNeeded(Job job)
        {
            if (job.def == JobDefOf.GotoWander) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.Wait_Wander) job.locomotionUrgency = LocomotionUrgency.Walk;
        }

        public static void SpawnPawnsForVisit(FetchMode mode, OnlineVisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                OnlineVisitManager.nonFactionPawns = GetCaravanPawns(FetchMode.Host, visitData);
                foreach (Pawn pawn in OnlineVisitManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                OnlineVisitManager.nonFactionPawns = GetMapPawns(FetchMode.Player, visitData);
                foreach (Pawn pawn in OnlineVisitManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }
        }

        public static List<byte[]> GetHumansForVisit(FetchMode mode)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn human in mapHumans)
                {
                    HumanData data = HumanScribeManager.HumanToString(human);
                    convertedList.Add(Serializer.ConvertObjectToBytes(data));
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn human in caravanHumans)
                {
                    HumanData data = HumanScribeManager.HumanToString(human);
                    convertedList.Add(Serializer.ConvertObjectToBytes(data));
                }

                return convertedList;
            }
        }

        public static List<byte[]> GetAnimalsForVisit(FetchMode mode)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn animal in mapAnimals)
                {
                    AnimalData data = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(Serializer.ConvertObjectToBytes(data));
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn animal in caravanAnimals)
                {
                    AnimalData data = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(Serializer.ConvertObjectToBytes(data));
                }

                return convertedList;
            }
        }

        public static bool GetPawnDraftState(Pawn pawn)
        {
            if (pawn.drafter == null) return false;
            else return pawn.drafter.Drafted;
        }

        public static List<Pawn> GetMapPawns(FetchMode mode, OnlineVisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in mapHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in mapAnimals) allPawns.Add(pawn);

                return allPawns.ToList();
            }

            else
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (byte[] compressedHuman in visitData.mapHumans)
                {
                    HumanData humanDetailsJSON = (HumanData)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in visitData.mapAnimals)
                {
                    AnimalData animalData = (AnimalData)Serializer.ConvertBytesToObject(compressedAnimal);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalData);
                    pawnList.Add(animal);
                }

                return pawnList.ToList();
            }
        }

        public static List<Pawn> GetCaravanPawns(FetchMode mode, OnlineVisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (byte[] compressedHuman in visitData.caravanHumans)
                {
                    HumanData humanDetailsJSON = (HumanData)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in visitData.caravanAnimals)
                {
                    AnimalData animalDetailsJSON = (AnimalData)Serializer.ConvertBytesToObject(compressedAnimal);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalDetailsJSON);
                    pawnList.Add(animal);
                }

                return pawnList.ToList();
            }

            else
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in caravanHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in caravanAnimals) allPawns.Add(pawn);

                return allPawns.ToList();
            }
        }

        public static List<Pawn> GetFactionPawnsSecure() { return OnlineVisitManager.factionPawns.ToList(); }

        public static List<Pawn> GetOtherFactionPawnsSecure() { return OnlineVisitManager.nonFactionPawns.ToList(); }
    }
}