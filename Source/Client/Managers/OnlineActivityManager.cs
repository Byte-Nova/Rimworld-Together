using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Shared;
using static Shared.CommonEnumerators;
using UnityEngine;

namespace GameClient
{
    public static class OnlineActivityManager
    {
        public static List<Pawn> factionPawns = new List<Pawn>();
        public static List<Pawn> nonFactionPawns = new List<Pawn>();
        public static List<Thing> mapThings = new List<Thing>();
        public static Map onlineMap;

        public static Thing queuedThing;
        public static int queuedTimeSpeed;
        public static WeatherDef queuedWeather;
        public static GameCondition queuedGameCondition;
        public static TimeSpeed maximumAllowedTimeSpeed = TimeSpeed.Fast;

        public static void ParseOnlineActivityPacket(Packet packet)
        {
            OnlineActivityData data = Serializer.ConvertBytesToObject<OnlineActivityData>(packet.contents);

            switch (data._stepMode)
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
                    OnlineManagerHelper.ReceivePawnOrder(data);
                    break;

                case OnlineActivityStepMode.Create:
                    OnlineManagerHelper.ReceiveCreationOrder(data);
                    break;

                case OnlineActivityStepMode.Destroy:
                    OnlineManagerHelper.ReceiveDestructionOrder(data);
                    break;

                case OnlineActivityStepMode.Damage:
                    OnlineManagerHelper.ReceiveDamageOrder(data);
                    break;

                case OnlineActivityStepMode.Hediff:
                    OnlineManagerHelper.ReceiveHediffOrder(data);
                    break;

                case OnlineActivityStepMode.TimeSpeed:
                    OnlineManagerHelper.ReceiveTimeSpeedOrder(data);
                    break;

                case OnlineActivityStepMode.GameCondition:
                    OnlineManagerHelper.ReceiveGameConditionOrder(data);
                    break;

                case OnlineActivityStepMode.Weather:
                    OnlineManagerHelper.ReceiveWeatherOrder(data);
                    break;

                case OnlineActivityStepMode.Kill:
                    OnlineManagerHelper.ReceiveKillOrder(data);
                    break;

                case OnlineActivityStepMode.Stop:
                    OnActivityStop();
                    break;
            }
        }

        public static void RequestOnlineActivity(OnlineActivityType toRequest)
        {
            if (!SessionValues.actionValues.EnableOnlineActivities)
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RTFeatureDisabled".Translate()));
                return;
            }

            else if (SessionValues.currentRealTimeEvent != OnlineActivityType.None)
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("RTAlreadyIRT".Translate()));
            }
            
            else
            {
                OnlineManagerHelper.ClearAllQueues();
                ClientValues.ToggleRealTimeHost(false);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("RTDialogServerWait".Translate()));

                OnlineActivityData data = new OnlineActivityData();
                data._stepMode = OnlineActivityStepMode.Request;
                data._activityType = toRequest;
                data._fromTile = Find.AnyPlayerHomeMap.Tile;
                data._toTile = SessionValues.chosenSettlement.Tile;
                data._caravanHumans = OnlineManagerHelper.GetActivityHumans();
                data._caravanAnimals = OnlineManagerHelper.GetActivityAnimals();

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OnlineActivityPacket), data);
                Network.listener.EnqueuePacket(packet);
            }
        }

        public static void RequestStopOnlineActivity()
        {
            OnlineActivityData data = new OnlineActivityData();
            data._stepMode = OnlineActivityStepMode.Stop;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OnlineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        private static void JoinMap(MapData mapData, OnlineActivityData activityData)
        {
            onlineMap = MapScribeManager.StringToMap(mapData, true, true, false, false, false, false);
            factionPawns = OnlineManagerHelper.GetCaravanPawns().ToList();
            mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

            OnlineManagerHelper.SpawnMapPawns(activityData);

            OnlineManagerHelper.EnterMap(activityData);

            //ALWAYS BEFORE RECEIVING ANY ORDERS BECAUSE THEY WILL BE IGNORED OTHERWISE
            SessionValues.ToggleOnlineFunction(activityData._activityType);
            OnlineManagerHelper.ReceiveTimeSpeedOrder(activityData);
        }

        private static void SendRequestedMap(OnlineActivityData data)
        {
            data._stepMode = OnlineActivityStepMode.Accept;
            data._mapHumans = OnlineManagerHelper.GetActivityHumans();
            data._mapAnimals = OnlineManagerHelper.GetActivityAnimals();
            data._timeSpeedOrder = OnlineManagerHelper.CreateTimeSpeedOrder();
            data._mapData = MapManager.ParseMap(onlineMap, true, false, false, true);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OnlineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnActivityRequest(OnlineActivityData data)
        {
            Action r1 = delegate
            {
                OnlineManagerHelper.ClearAllQueues();
                ClientValues.ToggleRealTimeHost(true);

                onlineMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == data._toTile).Map;
                factionPawns = OnlineManagerHelper.GetMapPawns().ToList();
                mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

                SendRequestedMap(data);

                OnlineManagerHelper.SpawnMapPawns(data);

                //ALWAYS LAST TO MAKE SURE WE DON'T SEND NON-NEEDED DETAILS BEFORE EVERYTHING IS READY
                SessionValues.ToggleOnlineFunction(data._activityType);
            };

            Action r2 = delegate
            {
                data._stepMode = OnlineActivityStepMode.Reject;
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OnlineActivityPacket), data);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo promptDialog = null;
            if (data._activityType == OnlineActivityType.Visit) promptDialog = new RT_Dialog_YesNo("RTVisitedBy".Translate(data._engagerName), r1, r2);
            else if (data._activityType == OnlineActivityType.Raid) promptDialog = new RT_Dialog_YesNo("RTRaidedBy".Translate(data._engagerName), r1, r2);

            DialogManager.PushNewDialog(promptDialog);
        }

        private static void OnActivityAccept(OnlineActivityData visitData)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate { JoinMap(visitData._mapData, visitData); };
            Action r2 = delegate { RequestStopOnlineActivity(); };
            if (!ModManager.CheckIfMapHasConflictingMods(visitData._mapData)) r1.Invoke();
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("RTMapUnknownModData".Translate(), r1, r2));
        }

        private static void OnActivityReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerRejected".Translate()));
        }

        private static void OnActivityUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerMustOnline".Translate()));
        }

        private static void OnActivityStop()
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;
            else
            {
                foreach (Pawn pawn in nonFactionPawns.ToArray())
                {
                    pawn.DeSpawn();

                    if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn)) Find.WorldPawns.RemovePawn(pawn);
                }

                if (!ClientValues.isRealTimeHost) CaravanExitMapUtility.ExitMapAndCreateCaravan(factionPawns, Faction.OfPlayer, onlineMap.Tile, Direction8Way.North, onlineMap.Tile);

                ClientValues.ToggleRealTimeHost(false);

                SessionValues.ToggleOnlineFunction(OnlineActivityType.None);

                DialogManager.PushNewDialog(new RT_Dialog_OK("RTOnlineEnded".Translate()));
            }
        }
    }

    public static class OnlineManagerHelper
    {
        //Create orders

        public static PawnOrder CreatePawnOrder(Pawn pawn, Job newJob)
        {
            PawnOrder pawnOrder = new PawnOrder();
            pawnOrder.pawnIndex = OnlineActivityManager.factionPawns.IndexOf(pawn);

            pawnOrder.targetCount = newJob.count;
            if (newJob.countQueue != null) pawnOrder.queueTargetCounts = newJob.countQueue.ToArray();

            pawnOrder.defName = newJob.def.defName;
            pawnOrder.targets = GetActionTargets(newJob);
            pawnOrder.targetIndexes = GetActionIndexes(newJob);
            pawnOrder.targetTypes = GetActionTypes(newJob);
            pawnOrder.targetFactions = GetActionTargetFactions(newJob);

            pawnOrder.queueTargetsA = GetQueuedActionTargets(newJob, 0);
            pawnOrder.queueTargetIndexesA = GetQueuedActionIndexes(newJob, 0);
            pawnOrder.queueTargetTypesA = GetQueuedActionTypes(newJob, 0);
            pawnOrder.queueTargetFactionsA = GetQueuedActionTargetFactions(newJob, 0);

            pawnOrder.queueTargetsB = GetQueuedActionTargets(newJob, 1);
            pawnOrder.queueTargetIndexesB = GetQueuedActionIndexes(newJob, 1);
            pawnOrder.queueTargetTypesB = GetQueuedActionTypes(newJob, 1);
            pawnOrder.queueTargetFactionsB = GetQueuedActionTargetFactions(newJob, 1);

            pawnOrder.isDrafted = GetPawnDraftState(pawn);
            pawnOrder.updatedPosition = ValueParser.IntVec3ToArray(pawn.Position);
            pawnOrder.updatedRotation = ValueParser.Rot4ToInt(pawn.Rotation);

            return pawnOrder;
        }

        //This function doesn't take into account non-host thing creation right now, handle with care

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
            destructionOrder.indexToDestroy = OnlineActivityManager.mapThings.IndexOf(thing);

            return destructionOrder;
        }

        public static DamageOrder CreateDamageOrder(DamageInfo damageInfo, Thing afectedThing)
        {
            DamageOrder damageOrder = new DamageOrder();
            damageOrder.defName = damageInfo.Def.defName;
            damageOrder.damageAmount = damageInfo.Amount;
            damageOrder.ignoreArmor = damageInfo.IgnoreArmor;
            damageOrder.armorPenetration = damageInfo.ArmorPenetrationInt;
            damageOrder.targetIndex = OnlineActivityManager.mapThings.IndexOf(afectedThing);
            if (damageInfo.Weapon != null) damageOrder.weaponDefName = damageInfo.Weapon.defName;
            if (damageInfo.HitPart != null) damageOrder.hitPartDefName = damageInfo.HitPart.def.defName;

            return damageOrder;
        }

        public static HediffOrder CreateHediffOrder(Hediff hediff, Pawn pawn, OnlineActivityApplyMode applyMode)
        {
            HediffOrder hediffOrder = new HediffOrder();
            hediffOrder.applyMode = applyMode;

            //Invert the enum because it needs to be mirrored for the non-host

            if (OnlineActivityManager.factionPawns.Contains(pawn))
            {
                hediffOrder.pawnFaction = OnlineActivityTargetFaction.NonFaction;
                hediffOrder.hediffTargetIndex = OnlineActivityManager.factionPawns.IndexOf(pawn);
            }

            else
            {
                hediffOrder.pawnFaction = OnlineActivityTargetFaction.Faction;
                hediffOrder.hediffTargetIndex = OnlineActivityManager.nonFactionPawns.IndexOf(pawn);
            }

            hediffOrder.hediffDefName = hediff.def.defName;
            if (hediff.Part != null) hediffOrder.hediffPartDefName = hediff.Part.def.defName;
            if (hediff.sourceDef != null) hediffOrder.hediffWeaponDefName = hediff.sourceDef.defName;
            hediffOrder.hediffSeverity = hediff.Severity;
            hediffOrder.hediffPermanent = hediff.IsPermanent();

            return hediffOrder;
        }

        public static TimeSpeedOrder CreateTimeSpeedOrder()
        {
            TimeSpeedOrder timeSpeedOrder = new TimeSpeedOrder();
            timeSpeedOrder.targetTimeSpeed = OnlineActivityManager.queuedTimeSpeed;
            timeSpeedOrder.targetMapTicks = RimworldManager.GetGameTicks();

            return timeSpeedOrder;
        }

        public static GameConditionOrder CreateGameConditionOrder(GameCondition gameCondition, OnlineActivityApplyMode applyMode)
        {
            GameConditionOrder gameConditionOrder = new GameConditionOrder();            
            gameConditionOrder.conditionDefName = gameCondition.def.defName;
            gameConditionOrder.duration = gameCondition.Duration;
            gameConditionOrder.applyMode = applyMode;

            return gameConditionOrder;
        }

        public static WeatherOrder CreateWeatherOrder(WeatherDef weatherDef)
        {
            WeatherOrder weatherOrder = new WeatherOrder();
            weatherOrder.weatherDefName = weatherDef.defName;

            return weatherOrder;
        }

        public static KillOrder CreateKillOrder(Thing instance)
        {
            KillOrder killOrder = new KillOrder();

            //Invert the enum because it needs to be mirrored for the non-host

            if (OnlineActivityManager.factionPawns.Contains(instance))
            {
                killOrder.pawnFaction = OnlineActivityTargetFaction.NonFaction;
                killOrder.killTargetIndex = OnlineActivityManager.factionPawns.IndexOf((Pawn)instance);
            }

            else
            {
                killOrder.pawnFaction = OnlineActivityTargetFaction.Faction;
                killOrder.killTargetIndex = OnlineActivityManager.nonFactionPawns.IndexOf((Pawn)instance);
            }

            return killOrder;
        }

        //Receive orders

        public static void ReceivePawnOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                Pawn pawn = OnlineActivityManager.nonFactionPawns[data._pawnOrder.pawnIndex];
                IntVec3 jobPositionStart = ValueParser.ArrayToIntVec3(data._pawnOrder.updatedPosition);
                Rot4 jobRotationStart = ValueParser.IntToRot4(data._pawnOrder.updatedRotation);
                ChangePawnTransform(pawn, jobPositionStart, jobRotationStart);
                SetPawnDraftState(pawn, data._pawnOrder.isDrafted);

                JobDef jobDef = RimworldManager.GetJobFromDef(data._pawnOrder.defName);
                LocalTargetInfo targetA = SetActionTargetsFromString(data._pawnOrder, 0);
                LocalTargetInfo targetB = SetActionTargetsFromString(data._pawnOrder, 1);
                LocalTargetInfo targetC = SetActionTargetsFromString(data._pawnOrder, 2);
                LocalTargetInfo[] targetQueueA = SetQueuedActionTargetsFromString(data._pawnOrder, 0);
                LocalTargetInfo[] targetQueueB = SetQueuedActionTargetsFromString(data._pawnOrder, 1);

                Job newJob = RimworldManager.SetJobFromDef(jobDef, targetA, targetB, targetC);
                newJob.count = data._pawnOrder.targetCount;
                if (data._pawnOrder.queueTargetCounts != null) newJob.countQueue = data._pawnOrder.queueTargetCounts.ToList();

                foreach (LocalTargetInfo target in targetQueueA) newJob.AddQueuedTarget(TargetIndex.A, target);
                foreach (LocalTargetInfo target in targetQueueB) newJob.AddQueuedTarget(TargetIndex.B, target);

                EnqueueThing(pawn);
                ChangeCurrentJob(pawn, newJob);
                ChangeJobSpeedIfNeeded(newJob);
            }
            catch { Logger.Warning($"Couldn't set order for pawn with index '{data._pawnOrder.pawnIndex}'"); }
        }

        //This function doesn't take into account non-host thing creation right now, handle with care

        public static void ReceiveCreationOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toSpawn;
            if (data._creationOrder.creationType == CreationType.Human)
            {
                HumanData humanData = Serializer.ConvertBytesToObject<HumanData>(data._creationOrder.dataToCreate);
                toSpawn = HumanScribeManager.StringToHuman(humanData);
                toSpawn.SetFaction(FactionValues.allyPlayer);
            }

            else if (data._creationOrder.creationType == CreationType.Animal)
            {
                AnimalData animalData = Serializer.ConvertBytesToObject<AnimalData>(data._creationOrder.dataToCreate);
                toSpawn = AnimalScribeManager.StringToAnimal(animalData);
                toSpawn.SetFaction(FactionValues.allyPlayer);
            }

            else
            {
                ThingData thingData = Serializer.ConvertBytesToObject<ThingData>(data._creationOrder.dataToCreate);
                toSpawn = ThingScribeManager.StringToItem(thingData);
            }

            EnqueueThing(toSpawn);

            //Request
            RimworldManager.PlaceThingIntoMap(toSpawn, OnlineActivityManager.onlineMap, ThingPlaceMode.Direct, false);
        }

        public static void ReceiveDestructionOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toDestroy = OnlineActivityManager.mapThings[data._destructionOrder.indexToDestroy];

            //Request
            if (ClientValues.isRealTimeHost) toDestroy.Destroy(DestroyMode.Deconstruct);
            else
            {
                EnqueueThing(toDestroy);
                toDestroy.Destroy(DestroyMode.Vanish);
            }
        }

        public static void ReceiveDamageOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                BodyPartRecord bodyPartRecord = new BodyPartRecord();
                bodyPartRecord.def = DefDatabase<BodyPartDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data._damageOrder.hitPartDefName);

                DamageDef damageDef = DefDatabase<DamageDef>.AllDefs.First(fetch => fetch.defName == data._damageOrder.defName);
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == data._damageOrder.weaponDefName);

                DamageInfo damageInfo = new DamageInfo(damageDef, data._damageOrder.damageAmount, data._damageOrder.armorPenetration, -1, null, bodyPartRecord, thingDef);
                damageInfo.SetIgnoreArmor(data._damageOrder.ignoreArmor);

                Thing toApplyTo = OnlineActivityManager.mapThings[data._damageOrder.targetIndex];

                EnqueueThing(toApplyTo);

                //Request
                toApplyTo.TakeDamage(damageInfo);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply damage order. Reason: {e}"); }
        }

        public static void ReceiveHediffOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                Pawn toTarget = null;
                if (data._hediffOrder.pawnFaction == OnlineActivityTargetFaction.Faction) toTarget = OnlineActivityManager.factionPawns[data._hediffOrder.hediffTargetIndex];
                else toTarget = OnlineActivityManager.nonFactionPawns[data._hediffOrder.hediffTargetIndex];

                EnqueueThing(toTarget);

                BodyPartRecord bodyPartRecord = toTarget.RaceProps.body.AllParts.FirstOrDefault(fetch => fetch.def.defName == data._hediffOrder.hediffPartDefName);

                if (data._hediffOrder.applyMode == OnlineActivityApplyMode.Add)
                {
                    HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder.hediffDefName);
                    Hediff toMake = HediffMaker.MakeHediff(hediffDef, toTarget, bodyPartRecord);
                    
                    if (data._hediffOrder.hediffWeaponDefName != null)
                    {
                        ThingDef source = DefDatabase<ThingDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder.hediffWeaponDefName);
                        toMake.sourceDef = source;
                        toMake.sourceLabel = source.label;
                    }

                    toMake.Severity = data._hediffOrder.hediffSeverity;

                    if (data._hediffOrder.hediffPermanent)
                    {
                        HediffComp_GetsPermanent hediffComp = toMake.TryGetComp<HediffComp_GetsPermanent>();
                        hediffComp.IsPermanent = true;
                    }

                    //Request
                    toTarget.health.AddHediff(toMake, bodyPartRecord);
                }

                else
                {
                    Hediff hediff = toTarget.health.hediffSet.hediffs.First(fetch => fetch.def.defName == data._hediffOrder.hediffDefName &&
                        fetch.Part.def.defName == bodyPartRecord.def.defName);

                    //Request
                    toTarget.health.RemoveHediff(hediff);
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply hediff order. Reason: {e}"); }
        }

        public static void ReceiveTimeSpeedOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                EnqueueTimeSpeed(data._timeSpeedOrder.targetTimeSpeed);
                RimworldManager.SetGameTicks(data._timeSpeedOrder.targetMapTicks);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply time speed order. Reason: {e}"); }
        }

        public static void ReceiveGameConditionOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                GameCondition gameCondition = null;

                if (data._gameConditionOrder.applyMode == OnlineActivityApplyMode.Add)
                {
                    GameConditionDef conditionDef = DefDatabase<GameConditionDef>.AllDefs.First(fetch => fetch.defName == data._gameConditionOrder.conditionDefName);
                    gameCondition = GameConditionMaker.MakeCondition(conditionDef);
                    gameCondition.Duration = data._gameConditionOrder.duration;
                    EnqueueGameCondition(gameCondition);

                    //Request
                    Find.World.gameConditionManager.RegisterCondition(gameCondition);
                }

                else
                {
                    gameCondition = Find.World.gameConditionManager.ActiveConditions.First(fetch => fetch.def.defName == data._gameConditionOrder.conditionDefName);
                    EnqueueGameCondition(gameCondition);

                    //Request
                    gameCondition.End();
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply game condition order. Reason: {e}"); }
        }

        public static void ReceiveWeatherOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == data._weatherOrder.weatherDefName);

                EnqueueWeather(weatherDef);

                //Request
                OnlineActivityManager.onlineMap.weatherManager.TransitionTo(weatherDef);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply weather order. Reason: {e}"); }
        }

        public static void ReceiveKillOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                Pawn toTarget = null;
                if (data._killOrder.pawnFaction == OnlineActivityTargetFaction.Faction) toTarget = OnlineActivityManager.factionPawns[data._killOrder.killTargetIndex];
                else toTarget = OnlineActivityManager.nonFactionPawns[data._killOrder.killTargetIndex];

                EnqueueThing(toTarget);

                //Request
                toTarget.Kill(null);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply kill order. Reason: {e}"); }
        }

        //Misc

        //This function doesn't take into account non-host thing creation right now, handle with care

        public static void AddThingToMap(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing) || DeepScribeHelper.CheckIfThingIsAnimal(thing))
            {
                if (ClientValues.isRealTimeHost) OnlineActivityManager.factionPawns.Add((Pawn)thing);
                else OnlineActivityManager.nonFactionPawns.Add((Pawn)thing);
            }
            else OnlineActivityManager.mapThings.Add(thing);
        }

        public static void RemoveThingFromMap(Thing thing)
        {
            if (OnlineActivityManager.factionPawns.Contains(thing)) OnlineActivityManager.factionPawns.Remove((Pawn)thing);
            else if (OnlineActivityManager.nonFactionPawns.Contains(thing)) OnlineActivityManager.nonFactionPawns.Remove((Pawn)thing);
            else OnlineActivityManager.mapThings.Remove(thing);
        }

        public static void ClearAllQueues()
        {
            ClearThingQueue();
            ClearTimeSpeedQueue();
            ClearWeatherQueue();
            ClearGameConditionQueue();
        }

        public static void EnqueueThing(Thing thing) { OnlineActivityManager.queuedThing = thing; }

        public static void EnqueueTimeSpeed(int timeSpeed) { OnlineActivityManager.queuedTimeSpeed = timeSpeed; }

        public static void EnqueueWeather(WeatherDef weatherDef) { OnlineActivityManager.queuedWeather = weatherDef; }

        public static void EnqueueGameCondition(GameCondition gameCondition) { OnlineActivityManager.queuedGameCondition = gameCondition; }

        public static void ClearThingQueue() { OnlineActivityManager.queuedThing = null; }

        public static void ClearTimeSpeedQueue() { OnlineActivityManager.queuedTimeSpeed = 0; }

        public static void ClearWeatherQueue() { OnlineActivityManager.queuedWeather = null; }

        public static void ClearGameConditionQueue() { OnlineActivityManager.queuedGameCondition = null; }

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

        public static LocalTargetInfo SetActionTargetsFromString(PawnOrder pawnOrder, int index)
        {
            LocalTargetInfo toGet = LocalTargetInfo.Invalid;

            try
            {
                switch (pawnOrder.targetTypes[index])
                {
                    case ActionTargetType.Thing:
                        toGet = new LocalTargetInfo(OnlineActivityManager.mapThings[pawnOrder.targetIndexes[index]]);
                        break;

                    case ActionTargetType.Human:
                        if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                        {
                            toGet = new LocalTargetInfo(OnlineActivityManager.factionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                        {
                            toGet = new LocalTargetInfo(OnlineActivityManager.nonFactionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        break;

                    case ActionTargetType.Animal:
                        if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                        {
                            toGet = new LocalTargetInfo(OnlineActivityManager.factionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                        {
                            toGet = new LocalTargetInfo(OnlineActivityManager.nonFactionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        break;

                    case ActionTargetType.Cell:
                        toGet = new LocalTargetInfo(ValueParser.StringToVector3(pawnOrder.targets[index]));
                        break;
                }
            }
            catch (Exception e) { Logger.Error(e.ToString()); }

            return toGet;
        }

        public static LocalTargetInfo[] SetQueuedActionTargetsFromString(PawnOrder pawnOrder, int index)
        {
            List<LocalTargetInfo> toGet = new List<LocalTargetInfo>();

            int[] actionTargetIndexes = null;
            string[] actionTargets = null;
            ActionTargetType[] actionTargetTypes = null;

            if (index == 0)
            {
                actionTargetIndexes = pawnOrder.queueTargetIndexesA.ToArray();
                actionTargets = pawnOrder.queueTargetsA.ToArray();
                actionTargetTypes = pawnOrder.queueTargetTypesA.ToArray();
            }

            else if (index == 1)
            {
                actionTargetIndexes = pawnOrder.queueTargetIndexesB.ToArray();
                actionTargets = pawnOrder.queueTargetsB.ToArray();
                actionTargetTypes = pawnOrder.queueTargetTypesB.ToArray();
            }

            for (int i = 0; i < actionTargets.Length; i++)
            {
                try
                {
                    switch (actionTargetTypes[index])
                    {
                        case ActionTargetType.Thing:
                            toGet.Add(new LocalTargetInfo(OnlineActivityManager.mapThings[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Human:
                            if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineActivityManager.factionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineActivityManager.nonFactionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            break;

                        case ActionTargetType.Animal:
                            if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineActivityManager.factionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineActivityManager.nonFactionPawns[pawnOrder.targetIndexes[index]]));
                            }
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
                        //Faction and non-faction pawns get inverted in here to send into the other side

                        if (OnlineActivityManager.factionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.NonFaction);
                        else if (OnlineActivityManager.nonFactionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.Faction);
                        else if (OnlineActivityManager.mapThings.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.None);
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetFactions.ToArray();
        }

        public static OnlineActivityTargetFaction[] GetQueuedActionTargetFactions(Job job, int index)
        {
            List<OnlineActivityTargetFaction> targetFactions = new List<OnlineActivityTargetFaction>();

            List<LocalTargetInfo> selectedQueue = new List<LocalTargetInfo>();
            if (index == 0) selectedQueue = job.targetQueueA;
            else if (index == 1) selectedQueue = job.targetQueueB;

            if (selectedQueue == null) return targetFactions.ToArray();
            for (int i = 0; i < selectedQueue.Count; i++)
            {
                try
                {
                    if (selectedQueue[i].Thing == null) targetFactions.Add(OnlineActivityTargetFaction.None);
                    else
                    {
                        //Faction and non-faction pawns get inverted in here to send into the other side

                        if (OnlineActivityManager.factionPawns.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.NonFaction);
                        else if (OnlineActivityManager.nonFactionPawns.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.Faction);
                        else if (OnlineActivityManager.mapThings.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.None);
                    }
                }
                catch { Logger.Error($"failed to parse {selectedQueue[i]}"); }
            }

            return targetFactions.ToArray();
        }

        public static int[] GetActionIndexes(Job job)
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
                        if (OnlineActivityManager.factionPawns.Contains(target.Thing)) targetIndexList.Add(OnlineActivityManager.factionPawns.IndexOf((Pawn)target.Thing));
                        else if (OnlineActivityManager.nonFactionPawns.Contains(target.Thing)) targetIndexList.Add(OnlineActivityManager.nonFactionPawns.IndexOf((Pawn)target.Thing));
                        else if (OnlineActivityManager.mapThings.Contains(target.Thing)) targetIndexList.Add(OnlineActivityManager.mapThings.IndexOf(target.Thing));
                    }
                }
                catch { Logger.Error($"failed to parse {target}"); }
            }

            return targetIndexList.ToArray();
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
                        if (OnlineActivityManager.factionPawns.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineActivityManager.factionPawns.IndexOf((Pawn)selectedQueue[i].Thing));
                        else if (OnlineActivityManager.nonFactionPawns.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineActivityManager.nonFactionPawns.IndexOf((Pawn)selectedQueue[i].Thing));
                        else if (OnlineActivityManager.mapThings.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineActivityManager.mapThings.IndexOf(selectedQueue[i].Thing));
                    }
                }
                catch { Logger.Error($"failed to parse {selectedQueue[i]}"); }
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

        public static void SetPawnDraftState(Pawn pawn, bool shouldBeDrafted)
        {
            try
            {
                pawn.drafter ??= new Pawn_DraftController(pawn);

                if (shouldBeDrafted) pawn.drafter.Drafted = true;
                else { pawn.drafter.Drafted = false; }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply pawn draft state for {pawn.Label}. Reason: {e}"); }
        }

        public static bool GetPawnDraftState(Pawn pawn)
        {
            if (pawn.drafter == null) return false;
            else return pawn.drafter.Drafted;
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

        public static void SpawnMapPawns(OnlineActivityData activityData)
        {
            if (ClientValues.isRealTimeHost)
            {
                OnlineActivityManager.nonFactionPawns = GetCaravanPawns(activityData).ToList();
                foreach (Pawn pawn in OnlineActivityManager.nonFactionPawns)
                {
                    if (activityData._activityType == OnlineActivityType.Visit) pawn.SetFaction(FactionValues.allyPlayer);
                    else if (activityData._activityType == OnlineActivityType.Raid) pawn.SetFaction(FactionValues.enemyPlayer);

                    //Initial position and rotation left to default since caravan doesn't have it stored
                    GenSpawn.Spawn(pawn, OnlineActivityManager.onlineMap.Center, OnlineActivityManager.onlineMap, Rot4.Random);
                }
            }

            else
            {
                OnlineActivityManager.nonFactionPawns = GetMapPawns(activityData).ToList();
                foreach (Pawn pawn in OnlineActivityManager.nonFactionPawns)
                {
                    if (activityData._activityType == OnlineActivityType.Visit) pawn.SetFaction(FactionValues.allyPlayer);
                    else if (activityData._activityType == OnlineActivityType.Raid) pawn.SetFaction(FactionValues.enemyPlayer);

                    //Initial position and rotation grabbed from online details
                    GenSpawn.Spawn(pawn, pawn.Position, OnlineActivityManager.onlineMap, pawn.Rotation);
                }
            }
        }

        public static Pawn[] GetMapPawns(OnlineActivityData activityData = null)
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapHumans = OnlineActivityManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineActivityManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
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

                foreach (HumanData humanData in activityData._mapHumans)
                {
                    Pawn human = HumanScribeManager.StringToHuman(humanData);
                    pawnList.Add(human);
                }

                foreach (AnimalData animalData in activityData._mapAnimals)
                {
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalData);
                    pawnList.Add(animal);
                }

                return pawnList.ToArray();
            }
        }

        public static Pawn[] GetCaravanPawns(OnlineActivityData activityData = null)
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> pawnList = new List<Pawn>();

                foreach (HumanData humanData in activityData._caravanHumans)
                {
                    Pawn human = HumanScribeManager.StringToHuman(humanData);
                    pawnList.Add(human);
                }

                foreach (AnimalData animalData in activityData._caravanAnimals)
                {
                    Pawn animal = AnimalScribeManager.StringToAnimal(animalData);
                    pawnList.Add(animal);
                }

                return pawnList.ToArray();
            }

            else
            {
                List<Pawn> caravanHumans = SessionValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> caravanAnimals = SessionValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> allPawns = new List<Pawn>();
                foreach (Pawn pawn in caravanHumans) allPawns.Add(pawn);
                foreach (Pawn pawn in caravanAnimals) allPawns.Add(pawn);

                return allPawns.ToArray();
            }
        }

        public static List<HumanData> GetActivityHumans()
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapHumans = OnlineActivityManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<HumanData> convertedList = new List<HumanData>();
                foreach (Pawn human in mapHumans)
                {
                    HumanData data = HumanScribeManager.HumanToString(human);
                    convertedList.Add(data);
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanHumans = SessionValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<HumanData> convertedList = new List<HumanData>();
                foreach (Pawn human in caravanHumans)
                {
                    HumanData data = HumanScribeManager.HumanToString(human);
                    convertedList.Add(data);
                }

                return convertedList;
            }
        }

        public static List<AnimalData> GetActivityAnimals()
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapAnimals = OnlineActivityManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<AnimalData> convertedList = new List<AnimalData>();
                foreach (Pawn animal in mapAnimals)
                {
                    AnimalData data = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(data);
                }

                return convertedList;
            }

            else
            {
                List<Pawn> caravanAnimals = SessionValues.chosenCaravan.PawnsListForReading
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<AnimalData> convertedList = new List<AnimalData>();
                foreach (Pawn animal in caravanAnimals)
                {
                    AnimalData data = AnimalScribeManager.AnimalToString(animal);
                    convertedList.Add(data);
                }

                return convertedList;
            }
        }

        public static bool CheckIfIgnoreThingSync(Thing toCheck)
        {
            if (toCheck is Projectile) return true;
            else if (toCheck is Mote) return true;
            else return false;
        }

        public static void EnterMap(OnlineActivityData activityData)
        {
            if (activityData._activityType == OnlineActivityType.Visit)
            {
                CaravanEnterMapUtility.Enter(SessionValues.chosenCaravan, OnlineActivityManager.onlineMap, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: false);
            }

            else if (activityData._activityType == OnlineActivityType.Raid)
            {
                SettlementUtility.Attack(SessionValues.chosenCaravan, SessionValues.chosenSettlement);
            }

            CameraJumper.TryJump(OnlineActivityManager.factionPawns[0].Position, OnlineActivityManager.onlineMap);
        }
    }
}
