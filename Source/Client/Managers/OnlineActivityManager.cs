using System;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse.AI;

namespace GameClient
{
    public static class OnlineActivityManager
    {
        public static Map activityMap = new Map();

        public static List<Thing> activityMapThings = new List<Thing>();

        public static List<Pawn> factionPawns = new List<Pawn>();

        public static List<Pawn> nonFactionPawns = new List<Pawn>();

        public static int gameTicksBeforeActivity;

        public static void ParsePacket(Packet packet)
        {
            OnlineActivityData data = Serializer.ConvertBytesToObject<OnlineActivityData>(packet.contents);

            switch (data._stepMode)
            {
                case OnlineActivityStepMode.Request:
                    OnActivityRequest(data);
                    break;

                case OnlineActivityStepMode.Ready:
                    OnActivityAccept(data);
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

                case OnlineActivityStepMode.Stop:
                    OnActivityStop();
                    break;

                case OnlineActivityStepMode.Jobs:
                    OnlineActivityOrders.ReceiveJobOrder(data);
                    break;

                case OnlineActivityStepMode.Create:
                    OnlineActivityOrders.ReceiveCreationOrder(data);
                    break;

                case OnlineActivityStepMode.Destroy:
                    OnlineActivityOrders.ReceiveDestructionOrder(data);
                    break;

                case OnlineActivityStepMode.Damage:
                    OnlineActivityOrders.ReceiveDamageOrder(data);
                    break;

                case OnlineActivityStepMode.Hediff:
                    OnlineActivityOrders.ReceiveHediffOrder(data);
                    break;

                case OnlineActivityStepMode.GameCondition:
                    OnlineActivityOrders.ReceiveGameConditionOrder(data);
                    break;

                case OnlineActivityStepMode.Weather:
                    OnlineActivityOrders.ReceiveWeatherOrder(data);
                    break;

                case OnlineActivityStepMode.TimeSpeed:
                    OnlineActivityOrders.ReceiveTimeSpeedOrder(data);
                    break;
            }
        }

        public static void RequestOnlineActivity(OnlineActivityType activityType)
        {
            OnlineActivityData onlineActivityData = new OnlineActivityData();
            onlineActivityData._stepMode = OnlineActivityStepMode.Request;
            onlineActivityData._activityType = activityType;
            onlineActivityData._fromTile = Find.AnyPlayerHomeMap.Tile;
            onlineActivityData._toTile = SessionValues.chosenSettlement.Tile;
            onlineActivityData._guestHumans = OnlineActivityManagerHelper.GetActivityHumans();
            onlineActivityData._guestAnimals = OnlineActivityManagerHelper.GetActivityAnimals();

            Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), onlineActivityData);
            Network.listener.EnqueuePacket(packet);

            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));
        }

        private static void OnActivityRequest(OnlineActivityData data)
        {
            Action r1 = delegate
            {
                data._stepMode = OnlineActivityStepMode.Accept;

                Map toGet = Find.WorldObjects.Settlements.First(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer).Map;
                data._mapFile = MapScribeManager.MapToString(toGet, true, true, true, true, true, true);

                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));
            };

            Action r2 = delegate
            {
                data._stepMode = OnlineActivityStepMode.Reject;
                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                Network.listener.EnqueuePacket(packet);
            };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo($"Requested activity from '{data._engagerName}'. Accept?", r1, r2));
        }

        private static void OnActivityAccept(OnlineActivityData data)
        {
            // We pause by default to allow the host to resume when ready
            RimworldManager.SetGameSpeed(TimeSpeed.Paused);

            SessionValues.ToggleOnlineActivity(data._activityType);
            OnlineActivityManagerHelper.SetActivityHost(data);
            OnlineActivityManagerHelper.SetActivityMap(data);
            OnlineActivityManagerHelper.SetActivityMapThings();
            OnlineActivityManagerHelper.SetFactionPawnsForActivity();
            OnlineActivityManagerHelper.SetNonFactionPawnsForActivity(data);
            gameTicksBeforeActivity = RimworldManager.GetGameTicks();

            // Sync current time with visitor and jump to it
            if (SessionValues.isActivityHost)
            {
                CameraJumper.TryJump(nonFactionPawns[0].Position, activityMap);

                data._mapFile = null;
                data._stepMode = OnlineActivityStepMode.TimeSpeed;
                data._timeSpeedOrder = OnlineActivityOrders.CreateTimeSpeedOrder();

                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                Network.listener.EnqueuePacket(packet);
            }

            // Send it back to host to let them know the visitor is ready and join map
            else
            {
                OnlineActivityManagerHelper.JoinActivityMap(data._activityType);

                data._mapFile = null;
                data._stepMode = OnlineActivityStepMode.Ready;            

                Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), data);
                Network.listener.EnqueuePacket(packet);
            }

            SessionValues.ToggleOnlineActivityReady(true);
            Threader.GenerateThread(Threader.Mode.Activity);

            Logger.Warning($"My pawns > {factionPawns.Count}");
            //foreach(Pawn pawn in OnlineActivityManagerHelper.factionPawns) Logger.Warning(pawn.def.defName);

            Logger.Warning($"Other pawns > {nonFactionPawns.Count}");
            //foreach(Pawn pawn in OnlineActivityManagerHelper.nonFactionPawns) Logger.Warning(pawn.def.defName);

            Logger.Warning($"Map things > {activityMapThings.Count}");
            //foreach(ThingDataFile thingData in OnlineActivityManagerHelper.activityMapThings) Logger.Warning(thingData.Hash);

            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK($"Should start {SessionValues.isActivityHost}"));
        }

        private static void OnActivityReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK($"Should cancel"));
        }

        private static void OnActivityUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error($"This user is currently unavailable! {SessionValues.isActivityHost}"));
        }

        private static void OnActivityStop()
        {
            CleanActivity();
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error($"Activity has ended! {SessionValues.isActivityHost}"));
        }

        private static void CleanActivity()
        {
            SessionValues.ToggleOnlineActivity(OnlineActivityType.None);

            foreach (Pawn pawn in nonFactionPawns) pawn.Destroy();

            if (!SessionValues.isActivityHost)
            {
                RimworldManager.SetGameTicks(gameTicksBeforeActivity);

                CaravanExitMapUtility.ExitMapAndCreateCaravan(factionPawns, 
                    Faction.OfPlayer, activityMap.Tile, Direction8Way.North, 
                    activityMap.Tile);
            }

            SessionValues.ToggleOnlineActivityHost(false);
            SessionValues.ToggleOnlineActivityReady(false);
        }
    }

    public static class OnlineActivityManagerHelper
    {
        public static void SetFactionPawnsForActivity()
        {
            if (SessionValues.isActivityHost)
            {
                foreach (Pawn pawn in OnlineActivityManager.activityMap.mapPawns.AllPawns.ToList())
                {
                    OnlineActivityManager.factionPawns.Add(pawn);
                }
            }

            else
            {
                foreach (Pawn pawn in SessionValues.chosenCaravan.PawnsListForReading.ToList())
                {
                    OnlineActivityManager.factionPawns.Add(pawn);
                }
            }
        }

        public static void SetNonFactionPawnsForActivity(OnlineActivityData data)
        {
            if (SessionValues.isActivityHost)
            {
                OnlineActivityManager.nonFactionPawns.Clear();

                foreach (HumanFile human in data._guestHumans)
                {
                    Pawn toSpawn = HumanScribeManager.StringToHuman(human);
                    OnlineActivityManager.nonFactionPawns.Add(toSpawn);
                }

                foreach (AnimalFile animal in data._guestAnimals)
                {
                    Pawn toSpawn = AnimalScribeManager.StringToAnimal(animal);
                    OnlineActivityManager.nonFactionPawns.Add(toSpawn);
                }

                // We spawn the visiting pawns now for the host
                foreach (Pawn pawn in OnlineActivityManager.nonFactionPawns)
                {
                    pawn.Position = OnlineActivityManager.activityMap.Center;
                    RimworldManager.PlaceThingIntoMap(pawn, OnlineActivityManager.activityMap);
                }
            }
            else OnlineActivityManager.nonFactionPawns = OnlineActivityManager.activityMap.mapPawns.AllPawns.ToList();

            // We set the faction of the other side depending on the activity type
            foreach (Pawn pawn in OnlineActivityManager.nonFactionPawns)
            {
                if (SessionValues.currentRealTimeEvent == OnlineActivityType.Visit) pawn.SetFactionDirect(FactionValues.allyPlayer);
                else pawn.SetFactionDirect(FactionValues.enemyPlayer);
            }
        }

        public static void AddPawnToMap(Pawn toAdd)
        {
            if (SessionValues.isActivityHost) OnlineActivityManager.factionPawns.Add(toAdd);
            else OnlineActivityManager.nonFactionPawns.Add(toAdd);
            OnlineActivityQueues.SetThingQueue(null);
        }

        public static void RemovePawnFromMap(Pawn toRemove)
        {
            if (SessionValues.isActivityHost) OnlineActivityManager.factionPawns.Remove(toRemove);
            else OnlineActivityManager.nonFactionPawns.Remove(toRemove);
            OnlineActivityQueues.SetThingQueue(null);
        }

        public static void AddThingToMap(Thing toAdd) 
        { 
            OnlineActivityManager.activityMapThings.Add(toAdd);
            OnlineActivityQueues.SetThingQueue(null);
        }

        public static void RemoveThingFromMap(Thing toRemove)
        {
            OnlineActivityManager.activityMapThings.Remove(toRemove);
            OnlineActivityQueues.SetThingQueue(null);
        }

        public static void SetActivityHost(OnlineActivityData data)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer) != null)
            {
                SessionValues.ToggleOnlineActivityHost(true);
            }
            else SessionValues.ToggleOnlineActivityHost(false);
        }

        public static void SetActivityMap(OnlineActivityData data)
        {
            if (SessionValues.isActivityHost) OnlineActivityManager.activityMap = Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer).Map;
            else OnlineActivityManager.activityMap = MapScribeManager.StringToMap(data._mapFile, true, true, true, true, true, true);
        }

        public static void SetActivityMapThings()
        {
            OnlineActivityManager.activityMapThings = OnlineActivityManager.activityMap.listerThings.AllThings;
        }

        public static Thing GetThingFromID(string id)
        {
            return OnlineActivityManager.activityMapThings.FirstOrDefault(fetch => fetch.ThingID == id);
        }

        public static Pawn GetPawnFromID(string id, OnlineActivityTargetFaction targetFaction)
        {
            if (targetFaction == OnlineActivityTargetFaction.Faction) return OnlineActivityManager.factionPawns.FirstOrDefault(fetch => fetch.ThingID == id);
            else if (targetFaction == OnlineActivityTargetFaction.NonFaction) return OnlineActivityManager.nonFactionPawns.FirstOrDefault(fetch => fetch.ThingID == id);
            else throw new IndexOutOfRangeException();
        }

        public static void JoinActivityMap(OnlineActivityType activityType)
        {
            if (activityType == OnlineActivityType.Visit)
            {
                CaravanEnterMapUtility.Enter(SessionValues.chosenCaravan, OnlineActivityManager.activityMap, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: false);
            }

            else if (activityType == OnlineActivityType.Raid)
            {
                SettlementUtility.Attack(SessionValues.chosenCaravan, SessionValues.chosenSettlement);
            }

            CameraJumper.TryJump(OnlineActivityManager.factionPawns[0].Position, OnlineActivityManager.activityMap);
        }

        public static HumanFile[] GetActivityHumans()
        {
            List<HumanFile> toGet = new List<HumanFile>();

            if (SessionValues.isActivityHost)
            {
                foreach (Pawn pawn in OnlineActivityManager.activityMap.mapPawns.AllPawns.Where(fetch => fetch.Faction == Faction.OfPlayer && DeepScribeHelper.CheckIfThingIsHuman(fetch)))
                {
                    toGet.Add(HumanScribeManager.HumanToString(pawn));
                }
            }

            else
            {
                foreach (Pawn pawn in SessionValues.chosenCaravan.PawnsListForReading.Where(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch)))
                {
                    toGet.Add(HumanScribeManager.HumanToString(pawn));
                }
            }

            return toGet.ToArray();
        }

        public static AnimalFile[] GetActivityAnimals()
        {
            List<AnimalFile> toGet = new List<AnimalFile>();

            if (SessionValues.isActivityHost)
            {
                foreach (Pawn pawn in OnlineActivityManager.activityMap.mapPawns.AllPawns.Where(fetch => fetch.Faction == Faction.OfPlayer && DeepScribeHelper.CheckIfThingIsAnimal(fetch)))
                {
                    toGet.Add(AnimalScribeManager.AnimalToString(pawn));
                }
            }

            else
            {
                foreach (Pawn pawn in SessionValues.chosenCaravan.PawnsListForReading.Where(fetch => fetch.Faction == Faction.OfPlayer && DeepScribeHelper.CheckIfThingIsAnimal(fetch)))
                {
                    toGet.Add(AnimalScribeManager.AnimalToString(pawn));
                }
            }

            return toGet.ToArray();
        }
    }

    public static class OnlineActivityJobs
    {
        public static async Task StartJobsTicker()
        {
            while (SessionValues.currentRealTimeEvent != OnlineActivityType.None)
            {
                try { GetPawnJobs(); }
                catch (Exception e) { Logger.Error($"Jobs tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMilliseconds(SessionValues.actionValues.OnlineActivityTickMS));
            }
        }

        public static void GetPawnJobs()
        {
            PawnOrderData pawnOrderData = new PawnOrderData();
            List<PawnOrderComponent> ordersToGet = new List<PawnOrderComponent>();
            foreach (Pawn pawn in OnlineActivityManager.factionPawns.ToArray())
            {
                PawnOrderComponent toGet = GetPawnJob(pawn);
                if (toGet != null) ordersToGet.Add(GetPawnJob(pawn));    
            }
            pawnOrderData._pawnOrders = ordersToGet.ToArray();

            OnlineActivityData onlineActivityData = new OnlineActivityData();
            onlineActivityData._stepMode = OnlineActivityStepMode.Jobs;
            onlineActivityData._pawnOrder = pawnOrderData;

            Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), onlineActivityData);
            Network.listener.EnqueuePacket(packet);
        }

        public static void SetPawnJobs(OnlineActivityData data)
        {
            foreach(PawnOrderComponent component in data._pawnOrder._pawnOrders)
            {
                Pawn pawn = OnlineActivityManagerHelper.GetPawnFromID(component._pawnId, OnlineActivityTargetFaction.NonFaction);
                IntVec3 jobPosition = ValueParser.ArrayToIntVec3(component._updatedPosition);
                Rot4 jobRotation = ValueParser.IntToRot4(component._updatedRotation);

                try
                {
                    JobDef jobDef = RimworldManager.GetJobFromDef(component._jobDefName);
                    LocalTargetInfo targetA = SetActionTargetsFromString(component, 0);
                    LocalTargetInfo targetB = SetActionTargetsFromString(component, 1);
                    LocalTargetInfo targetC = SetActionTargetsFromString(component, 2);

                    Job newJob = RimworldManager.SetJobFromDef(jobDef, targetA, targetB, targetC);
                    newJob.count = component._jobThingCount;

                    if (CheckIfJobsAreTheSame(pawn.CurJob, newJob)) continue;
                    else
                    {
                        SetPawnTransform(pawn, jobPosition, jobRotation);
                        SetPawnDraftState(pawn, component._isDrafted);

                        OnlineActivityQueues.SetThingQueue(pawn);
                        ChangeCurrentJob(pawn, newJob);
                        ChangeJobSpeedIfNeeded(newJob);
                    }
                }

                // If the job fails to parse we still want to move the pawn around
                catch
                {
                    SetPawnTransform(pawn, jobPosition, jobRotation);
                    SetPawnDraftState(pawn, component._isDrafted);
                }   
            }
        }

        public static PawnOrderComponent GetPawnJob(Pawn pawn)
        {
            PawnOrderComponent pawnOrder = new PawnOrderComponent();
            pawnOrder._pawnId = pawn.ThingID;

            Job pawnJob = pawn.CurJob;
            if (pawnJob == null) return null;

            pawnOrder._jobDefName = pawnJob.def.defName;
            pawnOrder._jobThingCount = pawnJob.count;
            pawnOrder._targetComponent.targets = GetActionTargets(pawnJob);
            pawnOrder._targetComponent.targetTypes = GetActionTypes(pawnJob);
            pawnOrder._targetComponent.targetFactions = GetActionTargetFactions(pawnJob);

            if (pawnJob.targetQueueA != null) Logger.Warning($"Queue A > {pawnJob.targetQueueA.Count}");
            if (pawnJob.targetQueueB != null) Logger.Warning($"Queue B > {pawnJob.targetQueueB.Count}");

            pawnOrder._isDrafted = GetPawnDraftState(pawn);
            pawnOrder._updatedPosition = ValueParser.IntVec3ToArray(pawn.Position);
            pawnOrder._updatedRotation = ValueParser.Rot4ToInt(pawn.Rotation);

            return pawnOrder;
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
                    else targetInfoList.Add(target.Thing.ThingID);
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetInfoList.ToArray();
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

        public static OnlineActivityTargetFaction[] GetActionTargetFactions(Job job)
        {
            List<OnlineActivityTargetFaction> targetFactions = new List<OnlineActivityTargetFaction>();

            for (int i = 0; i < 3; i++)
            {
                LocalTargetInfo target = null;
                if (i == 0) target = job.targetA;
                else if (i == 1) target = job.targetB;
                else if (i == 2) target = job.targetC;

                try
                {
                    if (target.Thing == null) targetFactions.Add(OnlineActivityTargetFaction.None);
                    else
                    {
                        // Faction and non-faction pawns get inverted in here to send into the other side
                        if (OnlineActivityManager.factionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.NonFaction);
                        else if (OnlineActivityManager.nonFactionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.Faction);
                        else if (OnlineActivityManager.activityMapThings.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.None);
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetFactions.ToArray();
        }

        public static bool GetPawnDraftState(Pawn pawn)
        {
            if (pawn.drafter == null) return false;
            else return pawn.drafter.Drafted;
        }

        public static void SetPawnTransform(Pawn pawn, IntVec3 pawnPosition, Rot4 pawnRotation)
        {
            pawn.Position = pawnPosition;
            pawn.Rotation = pawnRotation;
            pawn.pather.Notify_Teleported_Int();
        }

        public static void SetPawnDraftState(Pawn pawn, bool isDrafted)
        {
            try
            {
                pawn.drafter ??= new Pawn_DraftController(pawn);

                if (isDrafted) pawn.drafter.Drafted = true;
                else { pawn.drafter.Drafted = false; }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply pawn draft state for {pawn.Label}. Reason: {e}"); }
        }

        public static LocalTargetInfo SetActionTargetsFromString(PawnOrderComponent pawnOrder, int index)
        {
            try
            {
                switch (pawnOrder._targetComponent.targetTypes[index])
                {
                    case ActionTargetType.Thing:
                        return new LocalTargetInfo(OnlineActivityManagerHelper.GetThingFromID(pawnOrder._targetComponent.targets[index]));

                    case ActionTargetType.Human:
                        return new LocalTargetInfo(OnlineActivityManagerHelper.GetPawnFromID(pawnOrder._targetComponent.targets[index], 
                            pawnOrder._targetComponent.targetFactions[index]));

                    case ActionTargetType.Animal:
                        return new LocalTargetInfo(OnlineActivityManagerHelper.GetPawnFromID(pawnOrder._targetComponent.targets[index], 
                            pawnOrder._targetComponent.targetFactions[index]));

                    case ActionTargetType.Cell:
                        return new LocalTargetInfo(ValueParser.StringToVector3(pawnOrder._targetComponent.targets[index]));
                }
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            throw new IndexOutOfRangeException();
        }

        public static void ChangeCurrentJob(Pawn pawn, Job newJob)
        {
            pawn.jobs.ClearQueuedJobs();
            if (pawn.jobs.curJob != null) pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);

            // TODO
            // Investigate if this can be implemented
            // pawn.Reserve(newJob.targetA, newJob);

            newJob.TryMakePreToilReservations(pawn, false);
            pawn.jobs.StartJob(newJob);
        }

        public static void ChangeJobSpeedIfNeeded(Job job)
        {
            if (job.def == JobDefOf.GotoWander) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.Wait_Wander) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.GotoSafeTemperature) job.locomotionUrgency = LocomotionUrgency.Walk;
            else if (job.def == JobDefOf.GotoAndBeSociallyActive) job.locomotionUrgency = LocomotionUrgency.Walk;
        }

        public static bool CheckIfJobsAreTheSame(Job jobA, Job jobB)
        {
            if (jobA == null) return false;
            else if (jobA.def.defName != jobB.def.defName) return false;
            else if (jobA.targetA != jobB.targetA) return false;
            else if (jobA.globalTarget != jobB.globalTarget) return false;
            else return true;
        }
    }

    public static class OnlineActivityOrders
    {
        private static bool CheckIfCanExecuteOrder()
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return false;
            else if (!SessionValues.isActivityReady) return false;
            else return true; 
        }

        public static CreationOrderData CreateCreationOrder(Thing thing)
        {
            CreationOrderData creationOrder = new CreationOrderData();

            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) creationOrder._creationType = CreationType.Human;
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) creationOrder._creationType = CreationType.Animal;
            else creationOrder._creationType = CreationType.Thing;

            // Modify position based on center cell because RimWorld doesn't store it by default
            thing.Position = thing.OccupiedRect().CenterCell;

            if (creationOrder._creationType == CreationType.Human) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString((Pawn)thing));
            else if (creationOrder._creationType == CreationType.Animal) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(AnimalScribeManager.AnimalToString((Pawn)thing));
            else if (creationOrder._creationType == CreationType.Thing) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(ThingScribeManager.ItemToString(thing, thing.stackCount));

            return creationOrder;
        }

        public static DestructionOrderData CreateDestructionOrder(Thing thing)
        {
            DestructionOrderData destructionOrder = new DestructionOrderData();
            destructionOrder._thingHash = thing.ThingID;
            
            return destructionOrder;
        }

        public static DamageOrderData CreateDamageOrder(DamageInfo damageInfo, Thing affectedThing)
        {
            DamageOrderData damageOrder = new DamageOrderData();
            damageOrder._defName = damageInfo.Def.defName;
            damageOrder._damageAmount = damageInfo.Amount;
            damageOrder._ignoreArmor = damageInfo.IgnoreArmor;
            damageOrder._armorPenetration = damageInfo.ArmorPenetrationInt;
            damageOrder.targetHash = affectedThing.ThingID;
            if (damageInfo.Weapon != null) damageOrder._weaponDefName = damageInfo.Weapon.defName;
            if (damageInfo.HitPart != null) damageOrder._hitPartDefName = damageInfo.HitPart.def.defName;

            return damageOrder;
        }

        public static HediffOrderData CreateHediffOrder(Hediff hediff, Pawn pawn, OnlineActivityApplyMode applyMode)
        {
            HediffOrderData hediffOrder = new HediffOrderData();
            hediffOrder._applyMode = applyMode;

            // We invert the enum because it needs to be mirrored for the non-host

            if (OnlineActivityManager.factionPawns.Contains(pawn))
            {
                hediffOrder._pawnFaction = OnlineActivityTargetFaction.NonFaction;
                hediffOrder.targetID = pawn.ThingID;
            }

            else
            {
                hediffOrder._pawnFaction = OnlineActivityTargetFaction.Faction;
                hediffOrder.targetID = pawn.ThingID;
            }

            hediffOrder._hediffComponent.DefName = hediff.def.defName;
            hediffOrder._hediffComponent.Severity = hediff.Severity;
            hediffOrder._hediffComponent.IsPermanent = hediff.IsPermanent();
            if (hediff.sourceDef != null) hediffOrder._hediffComponent.WeaponDefName = hediff.sourceDef.defName;
            if (hediff.Part != null)
            {
                hediffOrder._hediffComponent.PartDefName = hediff.Part.def.defName;
                hediffOrder._hediffComponent.PartLabel = hediff.Part.Label;
            }

            return hediffOrder;
        }

        public static GameConditionOrderData CreateGameConditionOrder(GameCondition gameCondition, OnlineActivityApplyMode applyMode)
        {
            GameConditionOrderData gameConditionOrder = new GameConditionOrderData();            
            gameConditionOrder._conditionDefName = gameCondition.def.defName;
            gameConditionOrder._duration = gameCondition.Duration;
            gameConditionOrder._applyMode = applyMode;

            return gameConditionOrder;
        }

        public static WeatherOrderData CreateWeatherOrder(WeatherDef weatherDef)
        {
            WeatherOrderData weatherOrder = new WeatherOrderData();
            weatherOrder._weatherDefName = weatherDef.defName;

            return weatherOrder;
        }

        public static TimeSpeedOrderData CreateTimeSpeedOrder()
        {
            TimeSpeedOrderData timeSpeedOrder = new TimeSpeedOrderData();
            timeSpeedOrder._targetTimeSpeed = OnlineActivityQueues.queuedTimeSpeed;
            timeSpeedOrder._targetMapTicks = RimworldManager.GetGameTicks();

            return timeSpeedOrder;
        }

        public static void ReceiveJobOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;
            else OnlineActivityJobs.SetPawnJobs(data);
        }

        public static void ReceiveCreationOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            Thing toCreate = null;

            switch(data._creationOrder._creationType)
            {
                case CreationType.Human:
                    HumanFile humanData = Serializer.ConvertBytesToObject<HumanFile>(data._creationOrder._dataToCreate);
                    toCreate = HumanScribeManager.StringToHuman(humanData);
                    toCreate.SetFaction(FactionValues.allyPlayer);
                    break;

                case CreationType.Animal:
                    AnimalFile animalData = Serializer.ConvertBytesToObject<AnimalFile>(data._creationOrder._dataToCreate);
                    toCreate = AnimalScribeManager.StringToAnimal(animalData);
                    toCreate.SetFaction(FactionValues.allyPlayer);
                    break;

                case CreationType.Thing:
                    ThingDataFile thingData = Serializer.ConvertBytesToObject<ThingDataFile>(data._creationOrder._dataToCreate);
                    toCreate = ThingScribeManager.StringToItem(thingData);
                    break;
            }

            // If we receive a hash that doesn't exist or we are host we ignore it
            if (toCreate != null && !SessionValues.isActivityHost)
            {
                OnlineActivityQueues.SetThingQueue(toCreate);
                RimworldManager.PlaceThingIntoMap(toCreate, OnlineActivityManager.activityMap, ThingPlaceMode.Direct, false);
            }
        }

        public static void ReceiveDestructionOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            // If we receive a hash that doesn't exist or we are host we ignore it
            Thing toDestroy = OnlineActivityManagerHelper.GetThingFromID(data._destructionOrder._thingHash);
            if (toDestroy != null && !SessionValues.isActivityHost)
            {
                OnlineActivityQueues.SetThingQueue(toDestroy);
                toDestroy.Destroy(DestroyMode.Vanish);
            }
        }

        public static void ReceiveDamageOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            try
            {
                BodyPartRecord bodyPartRecord = new BodyPartRecord();
                bodyPartRecord.def = DefDatabase<BodyPartDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data._damageOrder._hitPartDefName);

                DamageDef damageDef = DefDatabase<DamageDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data._damageOrder._defName);
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data._damageOrder._weaponDefName);

                DamageInfo damageInfo = new DamageInfo(damageDef, data._damageOrder._damageAmount, data._damageOrder._armorPenetration, -1, null, bodyPartRecord, thingDef);
                damageInfo.SetIgnoreArmor(data._damageOrder._ignoreArmor);

                // If we receive a hash that doesn't exist or we are host we ignore it
                Thing toApplyTo = OnlineActivityManagerHelper.GetThingFromID(data._damageOrder.targetHash);
                if (toApplyTo != null && !SessionValues.isActivityHost)
                {
                    OnlineActivityQueues.SetThingQueue(toApplyTo);
                    toApplyTo.TakeDamage(damageInfo);
                }   
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply damage order. Reason: {e}"); }
        }

        public static void ReceiveHediffOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;
            
            try
            {
                Pawn toTarget = null;
                if (data._hediffOrder._pawnFaction == OnlineActivityTargetFaction.Faction) toTarget = OnlineActivityManagerHelper.GetPawnFromID(data._hediffOrder.targetID, OnlineActivityTargetFaction.Faction);
                else toTarget = OnlineActivityManagerHelper.GetPawnFromID(data._hediffOrder.targetID, OnlineActivityTargetFaction.NonFaction);

                // If we receive a hash that doesn't exist or we are host we ignore it
                if (toTarget != null && !SessionValues.isActivityHost)
                {
                    OnlineActivityQueues.SetThingQueue(toTarget);

                    BodyPartRecord bodyPartRecord = toTarget.RaceProps.body.AllParts.FirstOrDefault(fetch => fetch.def.defName == data._hediffOrder._hediffComponent.PartDefName &&
                        fetch.Label == data._hediffOrder._hediffComponent.PartLabel);

                    if (data._hediffOrder._applyMode == OnlineActivityApplyMode.Add)
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder._hediffComponent.DefName);
                        Hediff toMake = HediffMaker.MakeHediff(hediffDef, toTarget, bodyPartRecord);
                        
                        if (data._hediffOrder._hediffComponent.WeaponDefName != null)
                        {
                            ThingDef source = DefDatabase<ThingDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder._hediffComponent.WeaponDefName);
                            toMake.sourceDef = source;
                            toMake.sourceLabel = source.label;
                        }

                        toMake.Severity = data._hediffOrder._hediffComponent.Severity;

                        if (data._hediffOrder._hediffComponent.IsPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = toMake.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        toTarget.health.AddHediff(toMake, bodyPartRecord);
                    }

                    else if (data._hediffOrder._applyMode == OnlineActivityApplyMode.Remove)
                    {
                        Hediff hediff = toTarget.health.hediffSet.hediffs.First(fetch => fetch.def.defName == data._hediffOrder._hediffComponent.DefName &&
                            fetch.Part.def.defName == bodyPartRecord.def.defName);

                        toTarget.health.RemoveHediff(hediff);
                    }
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply hediff order. Reason: {e}"); }
        }

        public static void ReceiveGameConditionOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            try
            {
                GameCondition gameCondition = null;

                if (data._gameConditionOrder._applyMode == OnlineActivityApplyMode.Add)
                {
                    GameConditionDef conditionDef = DefDatabase<GameConditionDef>.AllDefs.First(fetch => fetch.defName == data._gameConditionOrder._conditionDefName);
                    gameCondition = GameConditionMaker.MakeCondition(conditionDef);
                    gameCondition.Duration = data._gameConditionOrder._duration;

                    OnlineActivityQueues.SetGameConditionQueue(gameCondition);
                    Find.World.gameConditionManager.RegisterCondition(gameCondition);
                }

                else
                {
                    gameCondition = Find.World.gameConditionManager.ActiveConditions.First(fetch => fetch.def.defName == data._gameConditionOrder._conditionDefName);
                    OnlineActivityQueues.SetGameConditionQueue(gameCondition);
                    gameCondition.End();
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply game condition order. Reason: {e}"); }
        }

        public static void ReceiveWeatherOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == data._weatherOrder._weatherDefName);

                OnlineActivityQueues.SetWeatherQueue(weatherDef);
                OnlineActivityManager.activityMap.weatherManager.TransitionTo(weatherDef);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply weather order. Reason: {e}"); }
        }

        public static void ReceiveTimeSpeedOrder(OnlineActivityData data)
        {
            if (!CheckIfCanExecuteOrder()) return;

            try
            {
                OnlineActivityQueues.SetTimeSpeedQueue(data._timeSpeedOrder._targetTimeSpeed);
                RimworldManager.SetGameTicks(data._timeSpeedOrder._targetMapTicks);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply time speed order. Reason: {e}"); }
        }
    }

    public static class OnlineActivityQueues
    {
        public static Thing queuedThing;

        public static GameCondition queuedGameCondition;

        public static WeatherDef queuedWeather;

        public static int queuedTimeSpeed;

        public static void SetThingQueue(Thing toSetTo) { queuedThing = toSetTo; }

        public static void SetGameConditionQueue(GameCondition toSetTo) { queuedGameCondition = toSetTo; }

        public static void SetWeatherQueue(WeatherDef toSetTo) { queuedWeather = toSetTo; }

        public static void SetTimeSpeedQueue(int toSetTo) { queuedTimeSpeed = toSetTo; }
    }

    public static class OnlineActivityPatches
    {
        public static bool CheckIfCanExecutePatch(Map map)
        {
            if (Network.state == ClientNetworkState.Disconnected) return false;
            else if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return false;
            else if (!SessionValues.isActivityReady) return false;
            else if (map != null && OnlineActivityManager.activityMap != map) return false;
            else return true;
        }

        public static bool CheckIfShouldExecutePatch(Thing toPatch, bool checkFactionPawns, bool checkNonFactionPawns, bool checkMapThings)
        {
            if (checkFactionPawns && OnlineActivityManager.factionPawns.Contains(toPatch)) return true;
            else if (checkNonFactionPawns && OnlineActivityManager.nonFactionPawns.Contains(toPatch)) return true;
            else if (checkMapThings && OnlineActivityManager.activityMapThings.Contains(toPatch)) return true;
            else return false;
        }

        public static bool CheckInverseIfShouldPatch(Thing toPatch, bool checkFactionPawns, bool checkNonFactionPawns, bool checkMapThings)
        {
            if (checkFactionPawns && OnlineActivityManager.factionPawns.Contains(toPatch)) return false;
            else if (checkNonFactionPawns && OnlineActivityManager.nonFactionPawns.Contains(toPatch)) return false;
            else if (checkMapThings && OnlineActivityManager.activityMapThings.Contains(toPatch)) return false;
            else return true;
        }

        public static bool CheckIfIgnoreThingSync(Thing toCheck)
        {
            if (toCheck is Projectile) return true;
            else if (toCheck is Mote) return true;
            else if (toCheck is Filth) return true;
            else return false;
        }
    }
}
