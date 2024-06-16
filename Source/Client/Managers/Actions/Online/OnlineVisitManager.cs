using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Shared;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using static Shared.CommonEnumerators;


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
                    OnlineVisitHelper.ReceiveOrder(visitData);
                    break;

                case OnlineVisitStepMode.Create:
                    OnlineVisitHelper.CreateThing(visitData);
                    break;

                case OnlineVisitStepMode.Destroy:
                    OnlineVisitHelper.DestroyThing(visitData);
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
            ClientValues.ToggleVisit(true);

            visitMap = MapScribeManager.StringToMap(mapData, true, true, false, true, false, true);
            factionPawns = OnlineVisitHelper.GetCaravanPawns(FetchMode.Player, null);
            mapThings = RimworldManager.GetThingsInMap(visitMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

            OnlineVisitHelper.SpawnPawnsForVisit(FetchMode.Player, visitData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, visitMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);
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

            MapData mapData = MapManager.ParseMap(visitMap, true, true, true, true);
            visitData.mapDetails = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
            Network.listener.dataQueue.Enqueue(packet);
        }
    }

    public static class OnlineVisitHelper
    {
        public static void ReceiveOrder(OnlineVisitData visitData)
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
                LocalTargetInfo localTargetInfoA = GetActionTargetFromString(visitData.pawnOrder.actionTargetA, visitData.pawnOrder.actionTargetType, visitData.pawnOrder.actionTargetIndex);
                Job newJob = RimworldManager.SetJobFromDef(jobDef, localTargetInfoA);

                ChangeCurrentJob(pawn, newJob);
                ChangeJobSpeedIfNeeded(newJob);
            }
            catch { Logger.Warning($"Couldn't set job for human {pawn.Name}"); }
        }

        public static PawnOrder CreatePawnOrder(Pawn pawn)
        {
            PawnOrder pawnOrder = new PawnOrder();
            pawnOrder.pawnIndex = OnlineVisitManager.factionPawns.IndexOf(pawn);

            if (pawn.jobs.curJob != null)
            {
                pawnOrder.defName = pawn.jobs.curJob.def.defName;
                pawnOrder.actionTargetA = GetActionTarget(pawn.jobs.curJob.targetA);
                pawnOrder.actionTargetIndex = GetActionTargetIndex(pawn.jobs.curJob.targetA);
                pawnOrder.actionTargetType = GetActionTargetType(pawn.jobs.curJob.targetA);
            }

            pawnOrder.isDrafted = GetPawnDraftState(pawn);
            pawnOrder.positionSync = ValueParser.Vector3ToString(pawn.Position);
            pawnOrder.rotationSync = ValueParser.Rot4ToInt(pawn.Rotation);

            return pawnOrder;
        }

        public static void CreateThing(OnlineVisitData visitData)
        {
            if (visitData.creationOrder.creationType == CreationType.Human)
            {
                HumanData data = (HumanData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                Pawn toSpawn = HumanScribeManager.StringToHuman(data);
                RimworldManager.PlaceThingInMap(toSpawn, OnlineVisitManager.visitMap);

                OnlineVisitManager.nonFactionPawns.Add(toSpawn);
                Logger.Warning($"Created! > {OnlineVisitManager.nonFactionPawns.IndexOf(toSpawn)}");
            }

            else if (visitData.creationOrder.creationType == CreationType.Animal)
            {
                AnimalData data = (AnimalData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                Pawn toSpawn = AnimalScribeManager.StringToAnimal(data);
                RimworldManager.PlaceThingInMap(toSpawn, OnlineVisitManager.visitMap);

                OnlineVisitManager.nonFactionPawns.Add(toSpawn);
                Logger.Warning($"Created! > {OnlineVisitManager.nonFactionPawns.IndexOf(toSpawn)}");
            }

            else
            {
                ItemData data = (ItemData)Serializer.ConvertBytesToObject(visitData.creationOrder.dataToCreate);
                Thing toSpawn = ThingScribeManager.StringToItem(data);
                RimworldManager.PlaceThingInMap(toSpawn, OnlineVisitManager.visitMap);

                OnlineVisitManager.mapThings.Add(toSpawn);
                Logger.Warning($"Created! > {OnlineVisitManager.mapThings.IndexOf(toSpawn)}");
            }
        }

        public static void DestroyThing(OnlineVisitData visitData)
        {
            try
            {
                Thing toDestroy = OnlineVisitManager.mapThings[visitData.destructionOrder.indexToDestroy];
                toDestroy.Destroy(DestroyMode.Vanish);

                Logger.Warning($"Destroyed! > {OnlineVisitManager.mapThings.IndexOf(toDestroy)}");
                OnlineVisitManager.mapThings.RemoveAt(visitData.destructionOrder.indexToDestroy);
            }
            catch { }
        }

        public static LocalTargetInfo GetActionTargetFromString(string toReadFrom, ActionTargetType type, int index)
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
                        target = new LocalTargetInfo(ValueParser.StringToVector3(toReadFrom));
                        break;
                }
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            return target;
        }

        public static string GetActionTarget(LocalTargetInfo targetInfo)
        {
            try
            {
                if (targetInfo.Thing == null) return ValueParser.Vector3ToString(targetInfo.Cell);
                else
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(targetInfo.Thing)) return Serializer.SerializeToString(HumanScribeManager.HumanToString(targetInfo.Pawn));
                    else if (DeepScribeHelper.CheckIfThingIsAnimal(targetInfo.Thing)) return Serializer.SerializeToString(AnimalScribeManager.AnimalToString(targetInfo.Pawn));
                    else return Serializer.SerializeToString(ThingScribeManager.ItemToString(targetInfo.Thing, 1));
                }
            }
            catch { Logger.Error($"failed to parse {targetInfo}"); }

            return null;
        }

        public static int GetActionTargetIndex(LocalTargetInfo targetInfo)
        {
            try
            {
                if (targetInfo.Thing == null) return 0;
                else
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(targetInfo.Thing)) return OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    else if (DeepScribeHelper.CheckIfThingIsAnimal(targetInfo.Thing)) return OnlineVisitManager.factionPawns.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                    else return OnlineVisitManager.mapThings.FirstIndexOf(fetch => fetch == targetInfo.Thing);
                }
            }
            catch { Logger.Error($"Failed to parse {targetInfo}"); }

            return 0;
        }

        public static ActionTargetType GetActionTargetType(LocalTargetInfo targetInfo)
        {
            try
            {
                if (targetInfo.Thing == null) return ActionTargetType.Cell;
                else
                {
                    if (DeepScribeHelper.CheckIfThingIsHuman(targetInfo.Thing)) return ActionTargetType.Human;
                    else if (DeepScribeHelper.CheckIfThingIsAnimal(targetInfo.Thing)) return ActionTargetType.Animal;
                    else return ActionTargetType.Thing;
                }
            }
            catch { Logger.Error($"failed to parse {targetInfo}"); }

            return ActionTargetType.Invalid;
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
            if (pawn.jobs.curJob != null) pawn.jobs.EndCurrentJob(JobCondition.None, false);

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