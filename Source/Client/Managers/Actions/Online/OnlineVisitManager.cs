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
            VisitDetailsJSON visitDetailsJSON = (VisitDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (visitDetailsJSON.visitStepMode)
            {
                case (int)VisitStepMode.Request:
                    OnVisitRequest(visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Accept:
                    OnVisitAccept(visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Reject:
                    OnVisitReject();
                    break;

                case (int)VisitStepMode.Unavailable:
                    OnVisitUnavailable();
                    break;

                case (int)VisitStepMode.Action:
                    VisitActionGetter.ReceiveActions(visitDetailsJSON);
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

                    VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
                    visitDetailsJSON.visitStepMode = (int)VisitStepMode.Request;
                    visitDetailsJSON.fromTile = Find.AnyPlayerHomeMap.Tile.ToString();
                    visitDetailsJSON.targetTile = ClientValues.chosenSettlement.Tile.ToString();
                    visitDetailsJSON.caravanHumans = OnlineVisitHelper.GetHumansForVisit(FetchMode.Player);
                    visitDetailsJSON.caravanAnimals = OnlineVisitHelper.GetAnimalsForVisit(FetchMode.Player);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                    Network.listener.dataQueue.Enqueue(packet);
                };

                var d1 = new RT_Dialog_YesNo("This feature is still in beta, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
        }

        private static void VisitMap(MapDetailsJSON mapDetailsJSON, VisitDetailsJSON visitDetailsJSON)
        {
            ClientValues.ToggleVisit(true);

            visitMap = OnlineVisitHelper.GetMapForVisit(FetchMode.Player, mapDetailsJSON);
            factionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Player, null);
            mapThings = OnlineVisitHelper.GetMapThings(visitMap);

            syncedTime = false;

            VisitThingHelper.SpawnPawnsForVisit(FetchMode.Player, visitDetailsJSON);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            Threader.GenerateThread(Threader.Mode.Visit);

            RT_Dialog_OK_Loop d1 = new RT_Dialog_OK_Loop(new string[]
            {
                "You are now in online visit mode!",
                "Visit mode allows you to visit another player's base",
                "To stop the visit use /sv in the chat"
            });
            DialogManager.PushNewDialog(d1);
        }

        public static void StopVisit()
        {
            //TODO
            //Implement this

            VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
            visitDetailsJSON.visitStepMode = (int)VisitStepMode.Stop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }

        private static void OnVisitRequest(VisitDetailsJSON visitDetailsJSON)
        {
            Action r1 = delegate
            {
                ClientValues.ToggleVisit(true);

                visitMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == int.Parse(visitDetailsJSON.targetTile)).Map;
                factionPawns = OnlineVisitHelper.GetMapPawns(FetchMode.Host, null);
                mapThings = OnlineVisitHelper.GetMapThings(visitMap);

                syncedTime = true;

                SendRequestedMap(visitDetailsJSON);
                VisitThingHelper.SpawnPawnsForVisit(FetchMode.Host, visitDetailsJSON);
                Threader.GenerateThread(Threader.Mode.Visit);
            };

            Action r2 = delegate
            {
                visitDetailsJSON.visitStepMode = (int)VisitStepMode.Reject;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                Network.listener.dataQueue.Enqueue(packet);
            };

            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Visited by {visitDetailsJSON.visitorName}, accept?", r1, r2);
            DialogManager.PushNewDialog(d1);
        }

        private static void OnVisitAccept(VisitDetailsJSON visitDetailsJSON)
        {
            DialogManager.PopWaitDialog();

            MapDetailsJSON mapDetailsJSON = (MapDetailsJSON)Serializer.ConvertBytesToObject(visitDetailsJSON.mapDetails);

            Action r1 = delegate { VisitMap(mapDetailsJSON, visitDetailsJSON); };
            if (ModManager.CheckIfMapHasConflictingMods(mapDetailsJSON))
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

        private static void SendRequestedMap(VisitDetailsJSON visitDetailsJSON)
        {
            visitDetailsJSON.visitStepMode = (int)VisitStepMode.Accept;
            visitDetailsJSON.mapHumans = OnlineVisitHelper.GetHumansForVisit(FetchMode.Host);
            visitDetailsJSON.mapAnimals = OnlineVisitHelper.GetAnimalsForVisit(FetchMode.Host);

            MapDetailsJSON mapDetailsJSON = MapManager.ParseMap(visitMap, true, true, true, true);
            visitDetailsJSON.mapDetails = Serializer.ConvertObjectToBytes(mapDetailsJSON);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }
    }

    public static class VisitThingHelper
    {
        public static void SpawnPawnsForVisit(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                OnlineVisitManager.nonFactionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Host, visitDetailsJSON);
                foreach (Pawn pawn in OnlineVisitManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineVisitManager.visitMap.Center, OnlineVisitManager.visitMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                OnlineVisitManager.nonFactionPawns = OnlineVisitHelper.GetMapPawns(FetchMode.Player, visitDetailsJSON);
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

                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
        }

        private static void ActionClockTick()
        {
            VisitDetailsJSON visitDetailsJSON = new VisitDetailsJSON();
            visitDetailsJSON.visitStepMode = (int)VisitStepMode.Action;
            visitDetailsJSON.mapTicks = OnlineVisitHelper.GetGameTicks();

            foreach (Pawn pawn in OnlineVisitHelper.GetFactionPawnsSecure())
            {
                try
                {
                    if (pawn.jobs.curJob == null)
                    {
                        visitDetailsJSON.pawnActionDefNames.Add("null");
                        visitDetailsJSON.actionTargetA.Add("null");
                        visitDetailsJSON.actionTargetIndex.Add(0);
                    }

                    else
                    {
                        visitDetailsJSON.pawnActionDefNames.Add(pawn.jobs.curJob.def.defName);
                        visitDetailsJSON.actionTargetA.Add(VisitActionHelper.ActionTargetToString(pawn.jobs.curJob.targetA, visitDetailsJSON));
                        visitDetailsJSON.actionTargetIndex.Add(OnlineVisitHelper.GetActionTargetIndex(pawn.jobs.curJob.targetA));
                    }

                    visitDetailsJSON.isDrafted.Add(OnlineVisitHelper.GetPawnDraftState(pawn));
                    visitDetailsJSON.positionSync.Add(OnlineVisitHelper.Vector3ToString(pawn.Position));
                    visitDetailsJSON.rotationSync.Add(OnlineVisitHelper.Rot4ToInt(pawn.Rotation));
                }
                catch { Log.Warning($"Couldn't get job for human {pawn.Name}"); }
            }

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
            Network.listener.dataQueue.Enqueue(packet);
        }

        public static void ReceiveActions(VisitDetailsJSON visitDetailsJSON)
        {
            if (!ClientValues.isInVisit) return;

            OnlineVisitHelper.SetGameTicks(visitDetailsJSON.mapTicks);

            Pawn[] otherPawns = OnlineVisitHelper.GetOtherFactionPawnsSecure();
            for (int i = 0; i < otherPawns.Count(); i++)
            {
                try
                {
                    JobDef jobDef = OnlineVisitHelper.TryGetJobDefForJob(otherPawns[i], visitDetailsJSON.pawnActionDefNames[i]);

                    if (jobDef == null) return;

                    LocalTargetInfo localTargetInfoA = OnlineVisitHelper.TryGetLocalTargetInfo(otherPawns[i],
                        visitDetailsJSON.actionTargetA[i], (ActionTargetType)visitDetailsJSON.actionTargetType[i],
                        visitDetailsJSON.actionTargetIndex[i]);

                    Job newJob = OnlineVisitHelper.TryCreateNewJob(otherPawns[i], jobDef, localTargetInfoA);

                    if (newJob == null) return;

                    IntVec3 jobPositionStart = OnlineVisitHelper.StringToVector3(visitDetailsJSON.positionSync[i]);
                    Rot4 jobRotationStart = OnlineVisitHelper.StringToRot4(visitDetailsJSON.rotationSync[i]);

                    VisitActionHelper.ChangeCurrentJobSpeedIfNeeded(newJob);
                    VisitActionHelper.HandlePawnDrafting(otherPawns[i], visitDetailsJSON.isDrafted[i]);
                    VisitActionHelper.ChangeCurrentJobIfNeeded(otherPawns[i], newJob, jobPositionStart, jobRotationStart);
                }
                catch { Log.Warning($"Couldn't set job for human {otherPawns[i].Name}"); }
            }
        }
    }

    public static class VisitActionHelper
    {
        public static string ActionTargetToString(LocalTargetInfo targetInfo, VisitDetailsJSON visitDetailsJSON)
        {
            try
            {
                if (targetInfo.Thing == null)
                {
                    visitDetailsJSON.actionTargetType.Add(((int)ActionTargetType.Cell));
                    return OnlineVisitHelper.Vector3ToString(targetInfo.Cell);
                }

                else
                {
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        visitDetailsJSON.actionTargetType.Add((int)ActionTargetType.Human);
                        return Serializer.SerializeToString(HumanScribeManager.HumanToString(targetInfo.Pawn));
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        visitDetailsJSON.actionTargetType.Add((int)ActionTargetType.Animal);
                        return Serializer.SerializeToString(AnimalScribeManager.AnimalToString(targetInfo.Pawn));
                    }

                    else
                    {
                        visitDetailsJSON.actionTargetType.Add((int)ActionTargetType.Thing);
                        return Serializer.SerializeToString(ThingScribeManager.ItemToString(targetInfo.Thing, 1));
                    }
                }
            }
            catch { Log.Error($"failed to parse {targetInfo}"); }

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
            catch(Exception e) { Log.Warning(e.ToString()); }
        }

        public static void TryChangePawnPosition(Pawn pawn, IntVec3 pawnPosition, Rot4 pawnRotation)
        {
            try
            {
                pawn.Position = pawnPosition;
                pawn.Rotation = pawnRotation;
                pawn.pather.Notify_Teleported_Int();
            }
            catch { Log.Warning($"Couldn't set position of {pawn.Name}"); }
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
                        pawn.jobs.StartJob(newJob, JobCondition.InterruptForced);
                    }
                }

                else
                {
                    TryChangePawnPosition(pawn, positionSync, rotationSync);
                    pawn.jobs.EndCurrentOrQueuedJob(pawn.jobs.curJob, JobCondition.InterruptForced);
                    pawn.jobs.StartJob(newJob, JobCondition.InterruptForced);
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
            catch { Log.Warning($"Couldn't get job def of human {pawnForJob.Name}"); }

            return null;
        }

        public static Job TryCreateNewJob(Pawn pawnForJob, JobDef jobDef, LocalTargetInfo localTargetA)
        {
            try { return JobMaker.MakeJob(jobDef, localTargetA); }
            catch { Log.Warning($"Couldn't create job for human {pawnForJob.Name}"); }

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
                        Log.Message(index.ToString());
                        target = new LocalTargetInfo(OnlineVisitManager.mapThings[index]);
                        break;

                    case ActionTargetType.Human:
                        Log.Message(index.ToString());
                        target = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[index]);
                        break;

                    case ActionTargetType.Animal:
                        Log.Message(index.ToString());
                        target = new LocalTargetInfo(OnlineVisitManager.nonFactionPawns[index]);
                        break;

                    case ActionTargetType.Cell:
                        string[] cellCoords = toReadFrom.Split('|');
                        IntVec3 cell = new IntVec3(int.Parse(cellCoords[0]), int.Parse(cellCoords[1]), int.Parse(cellCoords[2]));
                        if (cell != null) target = new LocalTargetInfo(cell);
                        break;
                }
            }
            catch { Log.Error($"Failed to get target from {toReadFrom}"); }

            return target;
        }

        public static LocalTargetInfo TryGetLocalTargetInfo(Pawn pawnForJob, string actionTarget, ActionTargetType type, int index)
        {
            try { return TryGetActionTargetFromString(actionTarget, type, index); }
            catch { Log.Warning($"Couldn't get job target for {pawnForJob.Label}"); }

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
                    if (TransferManagerHelper.CheckIfThingIsHuman(targetInfo.Thing))
                    {
                        toReturn = OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }

                    else if (TransferManagerHelper.CheckIfThingIsAnimal(targetInfo.Thing))
                    {
                        toReturn = OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }

                    else
                    {
                        toReturn = OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    }
                }
            }
            catch { Log.Error($"failed to parse {targetInfo}"); }

            return toReturn;
        }

        public static Map GetMapForVisit(FetchMode mode, MapDetailsJSON mapDetailsJSON)
        {
            if (mode == FetchMode.Host) return MapScribeManager.StringToMap(mapDetailsJSON, false, false, false, false, false, false);
            else return MapScribeManager.StringToMap(mapDetailsJSON, true, false, true, false, true, false);
        }

        public static List<byte[]> GetHumansForVisit(FetchMode mode)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn human in mapHumans)
                {
                    HumanDetailsJSON details = HumanScribeManager.HumanToString(human);
                    convertedList.Add(Serializer.ConvertObjectToBytes(details));
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn human in caravanHumans)
                {
                    HumanDetailsJSON details = HumanScribeManager.HumanToString(human);
                    convertedList.Add(Serializer.ConvertObjectToBytes(details));
                }

                return convertedList;
            }
        }

        public static List<byte[]> GetAnimalsForVisit(FetchMode mode)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn animal in mapAnimals)
                {
                    AnimalDetailsJSON details = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(Serializer.ConvertObjectToBytes(details));
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<byte[]> convertedList = new List<byte[]>();
                foreach (Pawn animal in caravanAnimals)
                {
                    AnimalDetailsJSON details = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(Serializer.ConvertObjectToBytes(details));
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

        public static Pawn[] GetMapPawns(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineVisitManager.visitMap.mapPawns.AllPawns
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch) && fetch.Faction == Faction.OfPlayer)
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

                foreach (byte[] compressedHuman in visitDetailsJSON.mapHumans)
                {
                    HumanDetailsJSON humanDetailsJSON = (HumanDetailsJSON)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in visitDetailsJSON.mapAnimals)
                {
                    AnimalDetailsJSON animalDetailsJSON = (AnimalDetailsJSON)Serializer.ConvertBytesToObject(compressedAnimal);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalDetailsJSON);
                    pawnList.Add(animal);
                }

                return pawnList.ToArray();
            }
        }

        public static Pawn[] GetCaravanPawns(FetchMode mode, VisitDetailsJSON visitDetailsJSON)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (byte[] compressedHuman in visitDetailsJSON.caravanHumans)
                {
                    HumanDetailsJSON humanDetailsJSON = (HumanDetailsJSON)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in visitDetailsJSON.caravanAnimals)
                {
                    AnimalDetailsJSON animalDetailsJSON = (AnimalDetailsJSON)Serializer.ConvertBytesToObject(compressedAnimal);
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalDetailsJSON);
                    pawnList.Add(animal);
                }

                return pawnList.ToArray();
            }

            else
            {
                List<Pawn> caravanHumans = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> caravanAnimals = ClientValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => TransferManagerHelper.CheckIfThingIsAnimal(fetch))
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
                if (TransferManagerHelper.CheckIfThingIsHuman(thing)) continue;
                else if (TransferManagerHelper.CheckIfThingIsAnimal(thing)) continue;
                else thingsInMap.Add(thing);
            }

            string toPrint = "";
            foreach(Thing thing in thingsInMap)
            {
                toPrint += $"{thing.def.defName}{Environment.NewLine}";
            }
            Log.Warning(toPrint);

            Debug.LogWarning(thingsInMap.Count());

            return thingsInMap.OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToArray();
        }

        public static IntVec3 StringToVector3(string data)
        {
            string[] dataSplit = data.Split('|');
            return new IntVec3(int.Parse(dataSplit[0]), int.Parse(dataSplit[1]), int.Parse(dataSplit[2]));
        }

        public static string Vector3ToString(IntVec3 data)
        {
            return $"{data.x}|{data.y}|{data.z}";
        }

        public static Rot4 StringToRot4(int data)
        {
            return new Rot4(data);
        }

        public static int Rot4ToInt(Rot4 data)
        {
            return data.AsInt;
        }
    }
}