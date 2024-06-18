using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Shared;
using static Shared.CommonEnumerators;
using UnityEngine;

namespace GameClient
{
    public static class OnlineManager
    {
        public static List<Pawn> factionPawns = new List<Pawn>();
        public static List<Pawn> nonFactionPawns = new List<Pawn>();
        public static List<Thing> mapThings = new List<Thing>();
        public static Thing queuedThing;
        public static Map onlineMap;
        public static bool isHost;

        public static void ParseOnlinePacket(Packet packet)
        {
            OnlineActivityData data = (OnlineActivityData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.activityStepMode)
            {
                case OnlineActivityStepMode.Request:
                    OnActivityRequest(data);
                    break;

                case OnlineActivityStepMode.Accept:
                    OnActivityAccept(data);
                    break;

                case OnlineActivityStepMode.Reject:
                    OnActivityReject();
                    break;

                case OnlineActivityStepMode.Unavailable:
                    OnActivityUnavailable();
                    break;

                case OnlineActivityStepMode.Action:
                    OnlineHelper.ReceivePawnOrder(data);
                    break;

                case OnlineActivityStepMode.Create:
                    OnlineHelper.ReceiveCreationOrder(data);
                    break;

                case OnlineActivityStepMode.Destroy:
                    OnlineHelper.ReceiveDestructionOrder(data);
                    break;

                case OnlineActivityStepMode.Damage:
                    OnlineHelper.ReceiveDamageOrder(data);
                    break;

                case OnlineActivityStepMode.Stop:
                    OnActivityStop();
                    break;
            }
        }

        public static void RequestOnlineActivity(OnlineActivityType toRequest)
        {
            if (ClientValues.currentRealTimeEvent != OnlineActivityType.None) DialogManager.PushNewDialog(new RT_Dialog_Error("You are already in a real time activity!"));
            else
            {
                Action r1 = delegate
                {
                    DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for visit response"));

                    OnlineActivityData data = new OnlineActivityData();
                    data.activityStepMode = OnlineActivityStepMode.Request;
                    data.activityType = toRequest;
                    data.fromTile = Find.AnyPlayerHomeMap.Tile;
                    data.targetTile = ClientValues.chosenSettlement.Tile;
                    data.caravanHumans = OnlineHelper.GetHumansForVisit(FetchMode.Player);
                    data.caravanAnimals = OnlineHelper.GetAnimalsForVisit(FetchMode.Player);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), data);
                    Network.listener.EnqueuePacket(packet);
                };

                var d1 = new RT_Dialog_YesNo("This feature is still in beta, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
        }

        private static void JoinMap(MapData mapData, OnlineActivityData visitData)
        {
            isHost = false;

            onlineMap = MapScribeManager.StringToMap(mapData, true, true, false, true, false, true);
            factionPawns = OnlineHelper.GetCaravanPawns(FetchMode.Player, null);
            mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

            OnlineHelper.SpawnPawnsForVisit(FetchMode.Player, visitData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, onlineMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            CameraJumper.TryJump(factionPawns[0].Position, onlineMap);

            ClientValues.ToggleOnlineFunction(visitData.activityType);
            RimworldManager.SetGameTicks(visitData.mapTicks);
        }

        public static void StopOnlineActivity()
        {
            OnlineActivityData visitData = new OnlineActivityData();
            visitData.activityStepMode = OnlineActivityStepMode.Stop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnActivityRequest(OnlineActivityData visitData)
        {
            Action r1 = delegate
            {
                isHost = true;
                onlineMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == visitData.targetTile).Map;
                factionPawns = OnlineHelper.GetMapPawns(FetchMode.Host, null);
                mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

                SendRequestedMap(visitData);
                OnlineHelper.SpawnPawnsForVisit(FetchMode.Host, visitData);
                ClientValues.ToggleOnlineFunction(visitData.activityType);
            };

            Action r2 = delegate
            {
                visitData.activityStepMode = OnlineActivityStepMode.Reject;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo promptDialog = null;
            if (visitData.activityType == OnlineActivityType.Visit) promptDialog = new RT_Dialog_YesNo($"Visited by {visitData.otherPlayerName}, accept?", r1, r2);
            else if (visitData.activityType == OnlineActivityType.Raid) promptDialog = new RT_Dialog_YesNo($"Raided by {visitData.otherPlayerName}, accept?", r1, r2);
            else if (visitData.activityType == OnlineActivityType.Misc) promptDialog = new RT_Dialog_YesNo($"Misc by {visitData.otherPlayerName}, accept?", r1, r2);

            DialogManager.PushNewDialog(promptDialog);
        }

        private static void OnActivityAccept(OnlineActivityData visitData)
        {
            DialogManager.PopWaitDialog();

            MapData mapData = (MapData)Serializer.ConvertBytesToObject(visitData.mapDetails);

            Action r1 = delegate { JoinMap(mapData, visitData); };
            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Map received but contains unknown mod data", r1));
            }
            else DialogManager.PushNewDialog(new RT_Dialog_OK("Map received", r1));
        }

        private static void OnActivityReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the visit!"));
        }

        private static void OnActivityUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must be online!"));
        }

        private static void OnActivityStop()
        {
            if (ClientValues.currentRealTimeEvent != OnlineActivityType.Visit) return;
            else
            {
                DialogManager.PushNewDialog(new RT_Dialog_OK("Visiting event ended"));

                foreach (Pawn pawn in nonFactionPawns.ToArray()) pawn.Destroy();

                ClientValues.ToggleOnlineFunction(OnlineActivityType.None);
            }
        }

        private static void SendRequestedMap(OnlineActivityData visitData)
        {
            visitData.activityStepMode = OnlineActivityStepMode.Accept;
            visitData.mapHumans = OnlineHelper.GetHumansForVisit(FetchMode.Host);
            visitData.mapAnimals = OnlineHelper.GetAnimalsForVisit(FetchMode.Host);
            visitData.mapTicks = RimworldManager.GetGameTicks();

            MapData mapData = MapManager.ParseMap(onlineMap, true, true, true, true);
            visitData.mapDetails = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }
    }

    public static class OnlineHelper
    {
        public static PawnOrder CreatePawnOrder(Pawn pawn)
        {
            PawnOrder pawnOrder = new PawnOrder();
            pawnOrder.pawnIndex = OnlineManager.factionPawns.IndexOf(pawn);

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

        public static CreationOrder CreateCreationOrder(Thing thing)
        {
            CreationOrder creationOrder = new CreationOrder();

            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) creationOrder.creationType = CreationType.Human;
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) creationOrder.creationType = CreationType.Animal;
            else creationOrder.creationType = CreationType.Thing;

            if (creationOrder.creationType == CreationType.Human) creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString((Pawn)thing));
            else if (creationOrder.creationType == CreationType.Animal) creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(AnimalScribeManager.AnimalToString((Pawn)thing));
            else
            {
                //Modify position based on center cell because RimWorld doesn't store it by default
                thing.Position = thing.OccupiedRect().CenterCell;
                creationOrder.dataToCreate = Serializer.ConvertObjectToBytes(ThingScribeManager.ItemToString(thing, thing.stackCount));
            }

            return creationOrder;
        }

        public static DestructionOrder CreateDestructionOrder(Thing thing)
        {
            DestructionOrder destructionOrder = new DestructionOrder();
            destructionOrder.indexToDestroy = OnlineManager.mapThings.IndexOf(thing);

            return destructionOrder;
        }

        public static DamageOrder CreateDamageOrder(DamageInfo damageInfo, Thing afectedThing)
        {
            DamageOrder damageOrder = new DamageOrder();
            damageOrder.defName = damageInfo.Def.defName;
            damageOrder.damageAmount = damageInfo.Amount;
            damageOrder.ignoreArmor = damageInfo.IgnoreArmor;
            damageOrder.armorPenetration = damageInfo.ArmorPenetrationInt;
            damageOrder.targetIndex = OnlineManager.mapThings.IndexOf(afectedThing);
            if (damageInfo.Weapon != null) damageOrder.weaponDefName = damageInfo.Weapon.defName;
            if (damageInfo.HitPart != null) damageOrder.hitPartDefName = damageInfo.HitPart.def.defName;

            return damageOrder;
        }

        public static void ReceivePawnOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;
            if (!OnlineManager.isHost) RimworldManager.SetGameTicks(data.mapTicks);

            List<Pawn> otherPawns = GetOtherFactionPawnsSecure();
            Pawn pawn = otherPawns[data.pawnOrder.pawnIndex];

            IntVec3 jobPositionStart = ValueParser.StringToVector3(data.pawnOrder.positionSync);
            Rot4 jobRotationStart = ValueParser.StringToRot4(data.pawnOrder.rotationSync);
            ChangePawnTransform(pawn, jobPositionStart, jobRotationStart);
            HandlePawnDrafting(pawn, data.pawnOrder.isDrafted);

            try
            {
                JobDef jobDef = RimworldManager.GetJobFromDef(data.pawnOrder.defName);
                LocalTargetInfo targetA = GetActionTargetsFromString(data.pawnOrder, 0);
                LocalTargetInfo targetB = GetActionTargetsFromString(data.pawnOrder, 1);
                LocalTargetInfo targetC = GetActionTargetsFromString(data.pawnOrder, 2);
                LocalTargetInfo[] targetQueueA = GetQueuedActionTargetsFromString(data.pawnOrder, 0);
                LocalTargetInfo[] targetQueueB = GetQueuedActionTargetsFromString(data.pawnOrder, 1);

                Job newJob = RimworldManager.SetJobFromDef(jobDef, targetA, targetB, targetC);
                newJob.count = data.pawnOrder.count;

                foreach (LocalTargetInfo target in targetQueueA) newJob.AddQueuedTarget(TargetIndex.A, target);
                foreach (LocalTargetInfo target in targetQueueB) newJob.AddQueuedTarget(TargetIndex.B, target);

                ChangeCurrentJob(pawn, newJob);
                ChangeJobSpeedIfNeeded(newJob);
            }
            catch { Logger.Warning($"Couldn't set job for human {pawn.Name}"); }
        }

        public static void ReceiveCreationOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toSpawn;

            if (data.creationOrder.creationType == CreationType.Human)
            {
                HumanData humanData = (HumanData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = HumanScribeManager.StringToHuman(humanData);
            }

            else if (data.creationOrder.creationType == CreationType.Animal)
            {
                AnimalData animalData = (AnimalData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = AnimalScribeManager.StringToAnimal(animalData);
            }

            else
            {
                ItemData thingData = (ItemData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = ThingScribeManager.StringToItem(thingData);
            }

            //Request
            if (!OnlineManager.isHost) EnqueueThing(toSpawn);

            RimworldManager.PlaceThingInMap(toSpawn, OnlineManager.onlineMap);
        }

        public static void ReceiveDestructionOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toDestroy = OnlineManager.mapThings[data.destructionOrder.indexToDestroy];

            //Request
            if (OnlineManager.isHost) toDestroy.Destroy(DestroyMode.Deconstruct);
            else
            {
                EnqueueThing(toDestroy);
                toDestroy.Destroy(DestroyMode.Vanish);
            }
        }

        public static void ReceiveDamageOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                BodyPartRecord bodyPartRecord = new BodyPartRecord();
                bodyPartRecord.def = DefDatabase<BodyPartDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data.damageOrder.hitPartDefName);

                DamageDef damageDef = DefDatabase<DamageDef>.AllDefs.First(fetch => fetch.defName == data.damageOrder.defName);
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data.damageOrder.weaponDefName);

                DamageInfo damageInfo = new DamageInfo(damageDef, data.damageOrder.damageAmount, data.damageOrder.armorPenetration, -1, null, bodyPartRecord, thingDef);
                damageInfo.SetIgnoreArmor(data.damageOrder.ignoreArmor);

                //Request
                if (!OnlineManager.isHost)
                {
                    Thing toApplyTo = OnlineManager.mapThings[data.damageOrder.targetIndex];
                    EnqueueThing(toApplyTo);
                    toApplyTo.TakeDamage(damageInfo);
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply damage order. Reason: {e}"); }
        }

        public static void AddToVisitList(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing))
            {
                if (OnlineManager.isHost) OnlineManager.factionPawns.Add((Pawn)thing);
                else OnlineManager.nonFactionPawns.Add((Pawn)thing);
            }

            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing))
            {
                if (OnlineManager.isHost) OnlineManager.factionPawns.Add((Pawn)thing);
                else OnlineManager.nonFactionPawns.Add((Pawn)thing);
            }

            else OnlineManager.mapThings.Add(thing);

            Logger.Warning($"Created! > {thing.def.defName}");
        }

        public static void RemoveFromVisitList(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) OnlineManager.nonFactionPawns.Remove((Pawn)thing);
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) OnlineManager.nonFactionPawns.Remove((Pawn)thing);
            else OnlineManager.mapThings.Remove(thing);

            Logger.Warning($"Destroyed! > {thing.def.defName}");
        }

        public static void EnqueueThing(Thing thing) { OnlineManager.queuedThing = thing; }

        public static void ClearQueue() { OnlineManager.queuedThing = null; }

        public static LocalTargetInfo GetActionTargetsFromString(PawnOrder pawnOrder, int index)
        {
            LocalTargetInfo toGet = LocalTargetInfo.Invalid;

            try
            {
                switch (pawnOrder.actionTypes[index])
                {
                    case ActionTargetType.Thing:
                        toGet = new LocalTargetInfo(OnlineManager.mapThings[pawnOrder.actionIndexes[index]]);
                        break;

                    case ActionTargetType.Human:
                        toGet = new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.actionIndexes[index]]);
                        break;

                    case ActionTargetType.Animal:
                        toGet = new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.actionIndexes[index]]);
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

            for (int i = 0; i < actionTargets.Length; i++)
            {
                try
                {
                    switch (actionTargetTypes[index])
                    {
                        case ActionTargetType.Thing:
                            toGet.Add(new LocalTargetInfo(OnlineManager.mapThings[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Human:
                            toGet.Add(new LocalTargetInfo(OnlineManager.nonFactionPawns[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Animal:
                            toGet.Add(new LocalTargetInfo(OnlineManager.nonFactionPawns[actionTargetIndexes[i]]));
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

            for (int i = 0; i < 3; i++)
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
                        if (DeepScribeHelper.CheckIfThingIsHuman(target.Thing)) targetIndexList.Add(OnlineManager.factionPawns.FirstIndexOf(fetch => fetch == target.Thing));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(target.Thing)) targetIndexList.Add(OnlineManager.factionPawns.FirstIndexOf(fetch => fetch == target.Thing));
                        else
                        {
                            pawnOrder.count = OnlineManager.mapThings.Find(fetch => fetch == target.Thing).stackCount;
                            targetIndexList.Add(OnlineManager.mapThings.FirstIndexOf(fetch => fetch == target.Thing));
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
                        if (DeepScribeHelper.CheckIfThingIsHuman(selectedQueue[i].Thing)) targetIndexList.Add(OnlineManager.factionPawns.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
                        else if (DeepScribeHelper.CheckIfThingIsAnimal(selectedQueue[i].Thing)) targetIndexList.Add(OnlineManager.factionPawns.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
                        else targetIndexList.Add(OnlineManager.mapThings.FirstIndexOf(fetch => fetch == selectedQueue[i].Thing));
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

            //TODO
            //Investigate if this can be implemented
            //pawn.Reserve(newJob.targetA, newJob);

            newJob.TryMakePreToilReservations(pawn, false);
            pawn.jobs.StartJob(newJob);
        }

        public static void ChangeJobSpeedIfNeeded(Job job)
        {
            if (job.def == JobDefOf.GotoWander) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.Wait_Wander) job.locomotionUrgency = LocomotionUrgency.Walk;
        }

        public static void SpawnPawnsForVisit(FetchMode mode, OnlineActivityData visitData)
        {
            if (mode == FetchMode.Host)
            {
                OnlineManager.nonFactionPawns = GetCaravanPawns(FetchMode.Host, visitData);
                foreach (Pawn pawn in OnlineManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineManager.onlineMap.Center, OnlineManager.onlineMap, Rot4.Random);
                }
            }

            else if (mode == FetchMode.Player)
            {
                OnlineManager.nonFactionPawns = GetMapPawns(FetchMode.Player, visitData);
                foreach (Pawn pawn in OnlineManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);
                    GenSpawn.Spawn(pawn, OnlineManager.onlineMap.Center, OnlineManager.onlineMap, Rot4.Random);
                }
            }
        }

        public static List<byte[]> GetHumansForVisit(FetchMode mode)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineManager.onlineMap.mapPawns.AllPawns
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
                List<Pawn> mapAnimals = OnlineManager.onlineMap.mapPawns.AllPawns
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

        public static List<Pawn> GetMapPawns(FetchMode mode, OnlineActivityData visitData)
        {
            if (mode == FetchMode.Host)
            {
                List<Pawn> mapHumans = OnlineManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch) && fetch.Faction == Faction.OfPlayer)
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineManager.onlineMap.mapPawns.AllPawns
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

        public static List<Pawn> GetCaravanPawns(FetchMode mode, OnlineActivityData visitData)
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

        public static List<Pawn> GetFactionPawnsSecure() { return OnlineManager.factionPawns.ToList(); }

        public static List<Pawn> GetOtherFactionPawnsSecure() { return OnlineManager.nonFactionPawns.ToList(); }
    }
}
