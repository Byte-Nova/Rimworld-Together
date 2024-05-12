using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using RimWorld.Planet;
using Shared;
using UnityEngine;
using Verse;
using Verse.AI;
using static Shared.CommonEnumerators;


namespace GameClient
{
    public static class OnlineVisitManager
    {
        public static Pawn[] factionPawns = new Pawn[] { };

        public static Pawn[] nonFactionPawns = new Pawn[] { };

        public static Thing[] mapThings = new Thing[] { };

        public static Map visitMap = new Map();

        public static bool syncedTime = false;

        public static void ParseVisitPacket(Packet packet)
        {
            VisitData visitData = (VisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (visitData.visitStepMode)
            {
                case (int)VisitStepMode.Request:
                    OnVisitRequest(visitData);
                    break;

                case (int)VisitStepMode.Accept:
                    OnVisitAccept(visitData);
                    break;

                case (int)VisitStepMode.Reject:
                    OnVisitReject();
                    break;

                case (int)VisitStepMode.Unavailable:
                    OnVisitUnavailable();
                    break;

                case (int)VisitStepMode.Action:
                    VisitActionGetter.ReceiveActions(visitData);
                    break;

                case (int)VisitStepMode.Stop:
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

                    VisitData visitData = new VisitData();
                    visitData.visitStepMode = (int)VisitStepMode.Request;
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

        private static void VisitMap(MapData mapData, VisitData visitData)
        {
            ClientValues.ToggleVisit(true);

            visitMap = OnlineVisitHelper.GetMapForVisit(mapData);
            factionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Player, null);
            mapThings = OnlineVisitHelper.GetMapThings(visitMap);

            syncedTime = false;

            VisitThingHelper.SpawnPawnsForVisit(FetchMode.Player, visitData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            Threader.GenerateThread(Threader.Mode.Visit);
        }

        public static void StopVisit()
        {
            VisitData visitData = new VisitData();
            visitData.visitStepMode = (int)VisitStepMode.Stop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }

        private static void OnVisitRequest(VisitData visitData)
        {
            Action r1 = delegate
            {
                ClientValues.ToggleVisit(true);

                visitMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == visitData.targetTile).Map;
                factionPawns = OnlineVisitHelper.GetMapPawns(FetchMode.Host, null);
                mapThings = OnlineVisitHelper.GetMapThings(visitMap);

                syncedTime = true;

                SendRequestedMap(visitData);
                VisitThingHelper.SpawnPawnsForVisit(FetchMode.Host, visitData);
                Threader.GenerateThread(Threader.Mode.Visit);
            };

            Action r2 = delegate
            {
                visitData.visitStepMode = (int)VisitStepMode.Reject;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                Network.listener.dataQueue.Enqueue(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Visited by {visitData.visitorName}, accept?", r1, r2);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnVisitAccept(VisitData visitData)
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

        private static void SendRequestedMap(VisitData visitData)
        {
            visitData.visitStepMode = (int)VisitStepMode.Accept;
            visitData.mapHumans = OnlineVisitHelper.GetHumansForVisit(FetchMode.Host);
            visitData.mapAnimals = OnlineVisitHelper.GetAnimalsForVisit(FetchMode.Host);

            MapData mapData = MapManager.ParseMap(visitMap, true, true, true, true);
            visitData.mapDetails = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }
    }

    public static class VisitThingHelper
    {
        public static void SpawnPawnsForVisit(FetchMode mode, VisitData visitData)
        {
            if (mode == FetchMode.Host)
            {
                OnlineVisitManager.nonFactionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Host, visitData);
                foreach (Pawn pawn in OnlineVisitManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                OnlineVisitManager.nonFactionPawns = OnlineVisitHelper.GetMapPawns(FetchMode.Player, visitData);
                foreach (Pawn pawn in OnlineVisitManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }
        }
    }

    public static class VisitActionGetter
    {
        public static void StartActionClock()
        {
            while (ClientValues.isInVisit)
            {
                Thread.Sleep(1000);

                ActionClockTick();
            }
        }

        private static void ActionClockTick()
        {
            VisitData visitData = new VisitData();
            visitData.visitStepMode = (int)VisitStepMode.Action;
            visitData.mapTicks = OnlineVisitHelper.GetGameTicks();

            foreach (Pawn pawn in OnlineVisitHelper.GetFactionPawnsSecure())
            {
                try
                {
                    if (pawn.jobs.curJob == null)
                    {
                        visitData.pawnActionDefNames.Add("null");
                        visitData.actionTargetA.Add("null");
                        visitData.actionTargetIndex.Add(0);
                    }

                    else
                    {
                        visitData.pawnActionDefNames.Add(pawn.jobs.curJob.def.defName);
                        visitData.actionTargetA.Add(VisitActionHelper.ActionTargetToString(pawn.jobs.curJob.targetA, visitData));
                        visitData.actionTargetIndex.Add(OnlineVisitHelper.GetActionTargetIndex(pawn.jobs.curJob.targetA));
                    }

                    visitData.isDrafted.Add(OnlineVisitHelper.GetPawnDraftState(pawn));
                    visitData.positionSync.Add(ValueParser.Vector3ToString(pawn.Position));
                    visitData.rotationSync.Add(ValueParser.Rot4ToInt(pawn.Rotation));
                }
                catch { Logger.Warning($"Couldn't get job for human {pawn.Name}"); }
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }

        public static void ReceiveActions(VisitData visitData)
        {
            if (!ClientValues.isInVisit) return;

            OnlineVisitHelper.SetGameTicks(visitData.mapTicks);

            Pawn[] otherPawns = OnlineVisitHelper.GetOtherFactionPawnsSecure();
            for (int i = 0; i < otherPawns.Count(); i++)
            {
                try
                {
                    JobDef jobDef = OnlineVisitHelper.TryGetJobDefForJob(otherPawns[i], visitData.pawnActionDefNames[i]);

                    if (jobDef == null) return;

                    LocalTargetInfo localTargetInfoA = OnlineVisitHelper.TryGetLocalTargetInfo(otherPawns[i],
                        visitData.actionTargetA[i], (ActionTargetType)visitData.actionTargetType[i],
                        visitData.actionTargetIndex[i]);

                    Job newJob = OnlineVisitHelper.TryCreateNewJob(otherPawns[i], jobDef, localTargetInfoA);

                    if (newJob == null) return;

                    IntVec3 jobPositionStart = ValueParser.StringToVector3(visitData.positionSync[i]);
                    Rot4 jobRotationStart = ValueParser.StringToRot4(visitData.rotationSync[i]);

                    VisitActionHelper.ChangeCurrentJobSpeedIfNeeded(newJob);
                    VisitActionHelper.HandlePawnDrafting(otherPawns[i], visitData.isDrafted[i]);
                    VisitActionHelper.ChangeCurrentJobIfNeeded(otherPawns[i], newJob, jobPositionStart, jobRotationStart);
                }
                catch { Logger.Warning($"Couldn't set job for human {otherPawns[i].Name}"); }
            }
        }
    }

    public static class VisitActionHelper
    {
        public static string ActionTargetToString(LocalTargetInfo targetInfo, VisitData visitData)
        {
            try
            {
                if (targetInfo.Thing == null)
                {
                    visitData.actionTargetType.Add(((int)ActionTargetType.Cell));
                    return ValueParser.Vector3ToString(targetInfo.Cell);
                }

                else
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        visitData.actionTargetType.Add((int)ActionTargetType.Human);
                        return Serializer.SerializeToString(HumanScribeManager.HumanToString(targetInfo.Pawn));
                    }

                    else if (DeepScribeHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        visitData.actionTargetType.Add((int)ActionTargetType.Animal);
                        return Serializer.SerializeToString(AnimalScribeManager.AnimalToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitData.actionTargetType.Add((int)ActionTargetType.Thing);
                        return Serializer.SerializeToString(ThingScribeManager.ItemToString(targetInfo.Thing, 1));
                    }
                }
            }
            catch { Logger.Error($"failed to parse {targetInfo}"); }

            return null;
        }

        public static void HandlePawnDrafting(Pawn pawn, bool shouldBeDrafted)
        {
            try
            {
                pawn.drafter ??= new Pawn_DraftController(pawn);

                if (shouldBeDrafted) pawn.drafter.Drafted = true;
                else { pawn.drafter.Drafted = false; }
            }
            catch(Exception e) { Logger.Warning(e.ToString()); }
        }

        public static void TryChangePawnPosition(Pawn pawn, IntVec3 pawnPosition, Rot4 pawnRotation)
        {
            try
            {
                pawn.Position = pawnPosition;
                pawn.Rotation = pawnRotation;
                pawn.pather.Notify_Teleported_Int();
            }
            catch { Logger.Warning($"Couldn't set position of {pawn.Name}"); }
        }

        public static void ChangeCurrentJobIfNeeded(Pawn pawn, Job newJob, IntVec3 positionSync, Rot4 rotationSync)
        {
            if (pawn.jobs.curJob == null) pawn.jobs.StartJob(newJob);
            else
            {
                if (pawn.jobs.curJob.def == newJob.def)
                {
                    if (pawn.jobs.curJob.targetA == newJob.targetA) return;
                    else
                    {
                        TryChangePawnPosition(pawn, positionSync, rotationSync);
                        pawn.jobs.EndCurrentOrQueuedJob(pawn.jobs.curJob, JobCondition.InterruptForced);
                        pawn.jobs.StartJob(newJob);
                    }
                }

                else
                {
                    TryChangePawnPosition(pawn, positionSync, rotationSync);
                    pawn.jobs.EndCurrentOrQueuedJob(pawn.jobs.curJob, JobCondition.InterruptForced);
                    pawn.jobs.StartJob(newJob);
                }
            }
        }

        public static void ChangeCurrentJobSpeedIfNeeded(Job job)
        {
            if (job.def == JobDefOf.GotoWander) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.Wait_Wander) job.locomotionUrgency = LocomotionUrgency.Walk;
        }
    }

    public static class OnlineVisitHelper
    {
        public static int GetGameTicks()
        {
            return Find.TickManager.TicksSinceSettle;
        }

        public static void SetGameTicks(int newGameTicks)
        {
            if (!OnlineVisitManager.syncedTime)
            {
                Find.TickManager.DebugSetTicksGame(newGameTicks);
            }
        }

        public static JobDef TryGetJobDefForJob(Pawn pawnForJob, string jobDefName)
        {
            try { return DefDatabase<JobDef>.AllDefs.ToArray().First(fetch => fetch.defName == jobDefName); }
            catch { Logger.Warning($"Couldn't get job def of human {pawnForJob.Name}"); }

            return null;
        }

        public static Job TryCreateNewJob(Pawn pawnForJob, JobDef jobDef, LocalTargetInfo localTargetA)
        {
            try { return JobMaker.MakeJob(jobDef, localTargetA); }
            catch { Logger.Warning($"Couldn't create job for human {pawnForJob.Name}"); }

            return null;
        }

        public static LocalTargetInfo TryGetActionTargetFromString(string toReadFrom, ActionTargetType type, int index)
        {
            LocalTargetInfo target = LocalTargetInfo.Invalid;

            try
            {
                switch (type)
                {
                    case ActionTargetType.Thing:
                        target = new LocalTargetInfo(OnlineVisitManager.mapThings[index]);
                        break;

                    case ActionTargetType.Human:
                        target = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[index]);
                        break;

                    case ActionTargetType.Animal:
                        target = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[index]);
                        break;

                    case ActionTargetType.Cell:
                        IntVec3 cell = ValueParser.StringToVector3(toReadFrom);
                        if (cell != null) target = new LocalTargetInfo(cell);
                        break;
                }
            }
            catch { Logger.Error($"Failed to get target from {toReadFrom}"); }

            return target;
        }

        public static LocalTargetInfo TryGetLocalTargetInfo(Pawn pawnForJob, string actionTarget, ActionTargetType type, int index)
        {
            try { return TryGetActionTargetFromString(actionTarget, type, index); }
            catch { Logger.Warning($"Couldn't get job target for {pawnForJob.Label}"); }

            return null;
        }

        public static int GetActionTargetIndex(LocalTargetInfo targetInfo)
        {
            int toReturn = 0;

            try
            {
                if (targetInfo.Thing == null) toReturn = 0;

                else
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        toReturn = OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }

                    else if (DeepScribeHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        toReturn = OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }

                    else
                    {
                        toReturn = OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }
                }
            }
            catch { Logger.Error($"failed to parse {targetInfo}"); }

            return toReturn;
        }

        public static Map GetMapForVisit(MapData mapData)
        {
            return MapScribeManager.StringToMap(mapData, true, true, false, true, false, true);
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

        public static Pawn[] GetFactionPawnsSecure()
        {
            return OnlineVisitManager.factionPawns.ToArray();
        }

        public static Pawn[] GetOtherFactionPawnsSecure()
        {
            return OnlineVisitManager.nonFactionPawns.ToArray();
        }

        public static bool GetPawnDraftState(Pawn pawn)
        {
            if (pawn.drafter == null) return false;
            else return pawn.drafter.Drafted;
        }

        public static Pawn[] GetMapPawns(FetchMode mode, VisitData visitData)
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

                return allPawns.ToArray();
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

                return pawnList.ToArray();
            }
        }

        public static Pawn[] GetCaravanPawns(FetchMode mode, VisitData visitData)
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

                return pawnList.ToArray();
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

                return allPawns.ToArray();
            }
        }

        public static Thing[] GetMapThings(Map map)
        {
            List<Thing> thingsInMap = new List<Thing>();
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (DeepScribeHelper.CheckIfThingIsHuman(thing)) continue;
                else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) continue;
                else thingsInMap.Add(thing);
            }

            return thingsInMap.OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToArray();
        }
    }
}