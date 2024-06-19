﻿using RimWorld.Planet;
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
using static UnityEngine.GraphicsBuffer;

namespace GameClient
{
    public static class OnlineManager
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
                OnlineManagerHelper.ClearAllQueues();
                ClientValues.ToggleRealTimeHost(false);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));

                OnlineActivityData data = new OnlineActivityData();
                data.activityStepMode = OnlineActivityStepMode.Request;
                data.activityType = toRequest;
                data.fromTile = Find.AnyPlayerHomeMap.Tile;
                data.targetTile = ClientValues.chosenSettlement.Tile;
                data.caravanHumans = OnlineManagerHelper.GetActivityHumanBytes();
                data.caravanAnimals = OnlineManagerHelper.GetActivityAnimalBytes();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), data);
                Network.listener.EnqueuePacket(packet);
            }
        }

        public static void RequestStopOnlineActivity()
        {
            OnlineActivityData visitData = new OnlineActivityData();
            visitData.activityStepMode = OnlineActivityStepMode.Stop;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void JoinMap(MapData mapData, OnlineActivityData activityData)
        {
            onlineMap = MapScribeManager.StringToMap(mapData, true, true, false, false, false, false);
            factionPawns = OnlineManagerHelper.GetCaravanPawns().ToList();
            mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

            OnlineManagerHelper.SpawnMapPawns(activityData);

            CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, onlineMap, CaravanEnterMode.Edge,
                CaravanDropInventoryMode.DoNotDrop, draftColonists: false);

            CameraJumper.TryJump(factionPawns[0].Position, onlineMap);

            //ALWAYS BEFORE RECEIVING ANY ORDERS BECAUSE THEY WILL BE IGNORED OTHERWISE
            ClientValues.ToggleOnlineFunction(activityData.activityType);

            OnlineManagerHelper.ReceiveTimeSpeedOrder(activityData);

            //OnlineManagerHelper.ReceiveWeatherOrder(activityData);

            //OnlineManagerHelper.ReceiveGameConditionOrder(activityData);
        }

        private static void SendRequestedMap(OnlineActivityData visitData)
        {
            visitData.activityStepMode = OnlineActivityStepMode.Accept;
            visitData.mapHumans = OnlineManagerHelper.GetActivityHumanBytes();
            visitData.mapAnimals = OnlineManagerHelper.GetActivityAnimalBytes();
            visitData.timeSpeedOrder = OnlineManagerHelper.CreateTimeSpeedOrder();
            //visitData.weatherOrder = OnlineManagerHelper.CreateWeatherOrder();
            //visitData.gameConditionOrder = OnlineManagerHelper.CreateGameConditionOrder();

            MapData mapData = MapManager.ParseMap(onlineMap, true, false, false, true);
            visitData.mapDetails = Serializer.ConvertObjectToBytes(mapData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), visitData);
            Network.listener.EnqueuePacket(packet);
        }

        private static void OnActivityRequest(OnlineActivityData activityData)
        {
            Action r1 = delegate
            {
                OnlineManagerHelper.ClearAllQueues();
                ClientValues.ToggleRealTimeHost(true);

                onlineMap = Find.WorldObjects.Settlements.Find(fetch => fetch.Tile == activityData.targetTile).Map;
                factionPawns = OnlineManagerHelper.GetMapPawns().ToList();
                mapThings = RimworldManager.GetThingsInMap(onlineMap).OrderBy(fetch => (fetch.PositionHeld.ToVector3() - Vector3.zero).sqrMagnitude).ToList();

                SendRequestedMap(activityData);
                OnlineManagerHelper.SpawnMapPawns(activityData);

                //ALWAYS LAST TO MAKE SURE WE DON'T SEND NON-NEEDED DETAILS BEFORE EVERYTHING IS READY
                ClientValues.ToggleOnlineFunction(activityData.activityType);
            };

            Action r2 = delegate
            {
                activityData.activityStepMode = OnlineActivityStepMode.Reject;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OnlineActivityPacket), activityData);
                Network.listener.EnqueuePacket(packet);
            };

            RT_Dialog_YesNo promptDialog = null;
            if (activityData.activityType == OnlineActivityType.Visit) promptDialog = new RT_Dialog_YesNo($"Visited by {activityData.otherPlayerName}, accept?", r1, r2);
            else if (activityData.activityType == OnlineActivityType.Raid) promptDialog = new RT_Dialog_YesNo($"Raided by {activityData.otherPlayerName}, accept?", r1, r2);
            else if (activityData.activityType == OnlineActivityType.Misc) promptDialog = new RT_Dialog_YesNo($"Misc by {activityData.otherPlayerName}, accept?", r1, r2);

            DialogManager.PushNewDialog(promptDialog);
        }

        private static void OnActivityAccept(OnlineActivityData visitData)
        {
            DialogManager.PopWaitDialog();

            MapData mapData = (MapData)Serializer.ConvertBytesToObject(visitData.mapDetails);

            Action r1 = delegate { JoinMap(mapData, visitData); };
            Action r2 = delegate { RequestStopOnlineActivity(); };
            if (!ModManager.CheckIfMapHasConflictingMods(mapData)) r1.Invoke();
            else DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, r2));
        }

        private static void OnActivityReject()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player rejected the activity!"));
        }

        private static void OnActivityUnavailable()
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must be online!"));
        }

        private static void OnActivityStop()
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;
            else
            {
                foreach (Pawn pawn in nonFactionPawns.ToArray())
                {
                    if (Find.WorldPawns.AllPawnsAliveOrDead.Contains(pawn)) Find.WorldPawns.RemovePawn(pawn);
                    pawn.Destroy();
                }

                if (!ClientValues.isRealTimeHost) CaravanExitMapUtility.ExitMapAndCreateCaravan(factionPawns, Faction.OfPlayer, 0, Direction8Way.North, onlineMap.Tile);

                ClientValues.ToggleRealTimeHost(false);

                ClientValues.ToggleOnlineFunction(OnlineActivityType.None);

                DialogManager.PushNewDialog(new RT_Dialog_OK("Online activity ended"));
            }
        }
    }

    public static class OnlineManagerHelper
    {
        //Create orders

        public static PawnOrder CreatePawnOrder(Pawn pawn, Job newJob)
        {
            //TODO
            //CAPTURE COUNT OF CERTAIN JOBS

            PawnOrder pawnOrder = new PawnOrder();
            pawnOrder.pawnIndex = OnlineManager.factionPawns.IndexOf(pawn);

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

        public static HediffOrder CreateHediffOrder(Hediff hediff, Pawn pawn, OnlineActivityApplyMode applyMode)
        {
            HediffOrder hediffOrder = new HediffOrder();
            hediffOrder.applyMode = applyMode;

            //Invert the enum because it needs to be mirrored for the non-host

            if (OnlineManager.factionPawns.Contains(pawn))
            {
                hediffOrder.pawnFaction = OnlineActivityTargetFaction.NonFaction;
                hediffOrder.hediffTargetIndex = OnlineManager.factionPawns.IndexOf(pawn);
            }

            else
            {
                hediffOrder.pawnFaction = OnlineActivityTargetFaction.Faction;
                hediffOrder.hediffTargetIndex = OnlineManager.nonFactionPawns.IndexOf(pawn);
            }

            hediffOrder.hediffDefName = hediff.def.defName;
            if (hediff.Part != null) hediffOrder.hediffPartDefName = hediff.Part.def.defName;
            hediffOrder.hediffSeverity = hediff.Severity;
            hediffOrder.hediffPermanent = hediff.IsPermanent();

            return hediffOrder;
        }

        public static TimeSpeedOrder CreateTimeSpeedOrder()
        {
            TimeSpeedOrder timeSpeedOrder = new TimeSpeedOrder();
            timeSpeedOrder.targetTimeSpeed = OnlineManager.queuedTimeSpeed;
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

        //Receive orders

        public static void ReceivePawnOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                Pawn pawn = OnlineManager.nonFactionPawns[data.pawnOrder.pawnIndex];
                IntVec3 jobPositionStart = ValueParser.ArrayToIntVec3(data.pawnOrder.updatedPosition);
                Rot4 jobRotationStart = ValueParser.IntToRot4(data.pawnOrder.updatedRotation);
                ChangePawnTransform(pawn, jobPositionStart, jobRotationStart);
                SetPawnDraftState(pawn, data.pawnOrder.isDrafted);

                JobDef jobDef = RimworldManager.GetJobFromDef(data.pawnOrder.defName);
                LocalTargetInfo targetA = SetActionTargetsFromString(data.pawnOrder, 0);
                LocalTargetInfo targetB = SetActionTargetsFromString(data.pawnOrder, 1);
                LocalTargetInfo targetC = SetActionTargetsFromString(data.pawnOrder, 2);
                LocalTargetInfo[] targetQueueA = SetQueuedActionTargetsFromString(data.pawnOrder, 0);
                LocalTargetInfo[] targetQueueB = SetQueuedActionTargetsFromString(data.pawnOrder, 1);

                Job newJob = RimworldManager.SetJobFromDef(jobDef, targetA, targetB, targetC);
                newJob.count = data.pawnOrder.count;
                newJob.countQueue = new List<int> { 0, 0, 0 };

                foreach (LocalTargetInfo target in targetQueueA) newJob.AddQueuedTarget(TargetIndex.A, target);
                foreach (LocalTargetInfo target in targetQueueB) newJob.AddQueuedTarget(TargetIndex.B, target);

                EnqueueThing(pawn);
                ChangeCurrentJob(pawn, newJob);
                ChangeJobSpeedIfNeeded(newJob);
            }
            catch { Logger.Warning($"Couldn't set order for pawn with index '{data.pawnOrder.pawnIndex}'"); }
        }

        //This function doesn't take into account non-host thing creation right now, handle with care

        public static void ReceiveCreationOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toSpawn;
            if (data.creationOrder.creationType == CreationType.Human)
            {
                HumanData humanData = (HumanData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = HumanScribeManager.StringToHuman(humanData);
                toSpawn.SetFaction(FactionValues.allyPlayer);
            }

            else if (data.creationOrder.creationType == CreationType.Animal)
            {
                AnimalData animalData = (AnimalData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = AnimalScribeManager.StringToAnimal(animalData);
                toSpawn.SetFaction(FactionValues.allyPlayer);
            }

            else
            {
                ItemData thingData = (ItemData)Serializer.ConvertBytesToObject(data.creationOrder.dataToCreate);
                toSpawn = ThingScribeManager.StringToItem(thingData);
            }

            //Request
            if (!ClientValues.isRealTimeHost)
            {
                EnqueueThing(toSpawn);
                RimworldManager.PlaceThingInMap(toSpawn, OnlineManager.onlineMap);
            }
        }

        public static void ReceiveDestructionOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            Thing toDestroy = OnlineManager.mapThings[data.destructionOrder.indexToDestroy];

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
                if (!ClientValues.isRealTimeHost)
                {
                    Thing toApplyTo = OnlineManager.mapThings[data.damageOrder.targetIndex];

                    EnqueueThing(toApplyTo);
                    toApplyTo.TakeDamage(damageInfo);
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply damage order. Reason: {e}"); }
        }

        public static void ReceiveHediffOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                Pawn toTarget = null;
                if (data.hediffOrder.pawnFaction == OnlineActivityTargetFaction.Faction) toTarget = OnlineManager.factionPawns[data.hediffOrder.hediffTargetIndex];
                else toTarget = OnlineManager.nonFactionPawns[data.hediffOrder.hediffTargetIndex];

                if (!ClientValues.isRealTimeHost)
                {
                    EnqueueThing(toTarget);

                    BodyPartRecord bodyPartRecord = toTarget.RaceProps.body.AllParts.FirstOrDefault(fetch => fetch.def.defName == data.hediffOrder.hediffPartDefName);

                    if (data.hediffOrder.applyMode == OnlineActivityApplyMode.Add)
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.First(fetch => fetch.defName == data.hediffOrder.hediffDefName);
                        Hediff toMake = HediffMaker.MakeHediff(hediffDef, toTarget, bodyPartRecord);
                        toMake.Severity = data.hediffOrder.hediffSeverity;
                        if (data.hediffOrder.hediffPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = toMake.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        //Request
                        toTarget.health.AddHediff(toMake, bodyPartRecord);
                    }

                    else
                    {
                        Hediff hediff = toTarget.health.hediffSet.hediffs.First(fetch => fetch.def.defName == data.hediffOrder.hediffDefName &&
                            fetch.Part.def.defName == bodyPartRecord.def.defName);

                        //Request
                        toTarget.health.RemoveHediff(hediff);
                    }
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply hediff order. Reason: {e}"); }
        }

        public static void ReceiveTimeSpeedOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                EnqueueTimeSpeed(data.timeSpeedOrder.targetTimeSpeed);
                RimworldManager.SetGameTicks(data.timeSpeedOrder.targetMapTicks);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply time speed order. Reason: {e}"); }
        }

        public static void ReceiveGameConditionOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                if (!ClientValues.isRealTimeHost)
                {
                    GameCondition gameCondition = null;

                    if (data.gameConditionOrder.applyMode == OnlineActivityApplyMode.Add)
                    {
                        GameConditionDef conditionDef = DefDatabase<GameConditionDef>.AllDefs.First(fetch => fetch.defName == data.gameConditionOrder.conditionDefName);
                        gameCondition = GameConditionMaker.MakeCondition(conditionDef);
                        gameCondition.Duration = data.gameConditionOrder.duration;
                        EnqueueGameCondition(gameCondition);

                        Find.World.gameConditionManager.RegisterCondition(gameCondition);
                    }

                    else
                    {
                        gameCondition = Find.World.gameConditionManager.ActiveConditions.First(fetch => fetch.def.defName == data.gameConditionOrder.conditionDefName);
                        EnqueueGameCondition(gameCondition);
                        gameCondition.End();
                    }
                }
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply game condition order. Reason: {e}"); }
        }

        public static void ReceiveWeatherOrder(OnlineActivityData data)
        {
            if (ClientValues.currentRealTimeEvent == OnlineActivityType.None) return;

            try
            {
                WeatherDef weatherDef = DefDatabase<WeatherDef>.AllDefs.First(fetch => fetch.defName == data.weatherOrder.weatherDefName);

                EnqueueWeather(weatherDef);
                OnlineManager.onlineMap.weatherManager.TransitionTo(weatherDef);
            }
            catch (Exception e) { Logger.Warning($"Couldn't apply weather order. Reason: {e}"); }
        }

        //Misc

        //This function doesn't take into account non-host thing creation right now, handle with care

        public static void AddThingToMap(Thing thing)
        {
            if (DeepScribeHelper.CheckIfThingIsHuman(thing) || DeepScribeHelper.CheckIfThingIsAnimal(thing))
            {
                if (ClientValues.isRealTimeHost) OnlineManager.factionPawns.Add((Pawn)thing);
                else OnlineManager.nonFactionPawns.Add((Pawn)thing);
            }
            else OnlineManager.mapThings.Add(thing);
        }

        public static void RemoveThingFromMap(Thing thing)
        {
            if (OnlineManager.factionPawns.Contains(thing)) OnlineManager.factionPawns.Remove((Pawn)thing);
            else if (OnlineManager.nonFactionPawns.Contains(thing)) OnlineManager.nonFactionPawns.Remove((Pawn)thing);
            else OnlineManager.mapThings.Remove(thing);
        }

        public static void ClearAllQueues()
        {
            ClearThingQueue();
            ClearTimeSpeedQueue();
            ClearWeatherQueue();
            ClearGameConditionQueue();
        }

        public static void EnqueueThing(Thing thing) { OnlineManager.queuedThing = thing; }

        public static void EnqueueTimeSpeed(int timeSpeed) { OnlineManager.queuedTimeSpeed = timeSpeed; }

        public static void EnqueueWeather(WeatherDef weatherDef) { OnlineManager.queuedWeather = weatherDef; }

        public static void EnqueueGameCondition(GameCondition gameCondition) { OnlineManager.queuedGameCondition = gameCondition; }

        public static void ClearThingQueue() { OnlineManager.queuedThing = null; }

        public static void ClearTimeSpeedQueue() { OnlineManager.queuedTimeSpeed = 0; }

        public static void ClearWeatherQueue() { OnlineManager.queuedWeather = null; }

        public static void ClearGameConditionQueue() { OnlineManager.queuedGameCondition = null; }

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
                        toGet = new LocalTargetInfo(OnlineManager.mapThings[pawnOrder.targetIndexes[index]]);
                        break;

                    case ActionTargetType.Human:
                        if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                        {
                            toGet = new LocalTargetInfo(OnlineManager.factionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                        {
                            toGet = new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        break;

                    case ActionTargetType.Animal:
                        if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                        {
                            toGet = new LocalTargetInfo(OnlineManager.factionPawns[pawnOrder.targetIndexes[index]]);
                        }
                        else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                        {
                            toGet = new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.targetIndexes[index]]);
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
                            toGet.Add(new LocalTargetInfo(OnlineManager.mapThings[actionTargetIndexes[i]]));
                            break;

                        case ActionTargetType.Human:
                            if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineManager.factionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            break;

                        case ActionTargetType.Animal:
                            if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.Faction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineManager.factionPawns[pawnOrder.targetIndexes[index]]));
                            }
                            else if (pawnOrder.targetFactions[index] == OnlineActivityTargetFaction.NonFaction)
                            {
                                toGet.Add(new LocalTargetInfo(OnlineManager.nonFactionPawns[pawnOrder.targetIndexes[index]]));
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

                        if (OnlineManager.factionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.NonFaction);
                        else if (OnlineManager.nonFactionPawns.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.Faction);
                        else if (OnlineManager.mapThings.Contains(target.Thing)) targetFactions.Add(OnlineActivityTargetFaction.None);
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

                        if (OnlineManager.factionPawns.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.NonFaction);
                        else if (OnlineManager.nonFactionPawns.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.Faction);
                        else if (OnlineManager.mapThings.Contains(selectedQueue[i].Thing)) targetFactions.Add(OnlineActivityTargetFaction.None);
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
                        if (OnlineManager.factionPawns.Contains(target.Thing)) targetIndexList.Add(OnlineManager.factionPawns.IndexOf((Pawn)target.Thing));
                        else if (OnlineManager.nonFactionPawns.Contains(target.Thing)) targetIndexList.Add(OnlineManager.nonFactionPawns.IndexOf((Pawn)target.Thing));
                        else if (OnlineManager.mapThings.Contains(target.Thing)) targetIndexList.Add(OnlineManager.mapThings.IndexOf(target.Thing));
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
                        if (OnlineManager.factionPawns.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineManager.factionPawns.IndexOf((Pawn)selectedQueue[i].Thing));
                        else if (OnlineManager.nonFactionPawns.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineManager.nonFactionPawns.IndexOf((Pawn)selectedQueue[i].Thing));
                        else if (OnlineManager.mapThings.Contains(selectedQueue[i].Thing)) targetIndexList.Add(OnlineManager.mapThings.IndexOf(selectedQueue[i].Thing));
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
                OnlineManager.nonFactionPawns = GetCaravanPawns(activityData).ToList();
                foreach (Pawn pawn in OnlineManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);

                    //Initial position and rotation left to default since caravan doesn't have it stored
                    GenSpawn.Spawn(pawn, OnlineManager.onlineMap.Center, OnlineManager.onlineMap, Rot4.Random);
                }
            }

            else
            {
                OnlineManager.nonFactionPawns = GetMapPawns(activityData).ToList();
                foreach (Pawn pawn in OnlineManager.nonFactionPawns)
                {
                    pawn.SetFaction(FactionValues.allyPlayer);

                    //Initial position and rotation grabbed from online details
                    GenSpawn.Spawn(pawn, pawn.Position, OnlineManager.onlineMap, pawn.Rotation);
                }
            }
        }

        public static Pawn[] GetMapPawns(OnlineActivityData activityData = null)
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapHumans = OnlineManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
                    .OrderBy(p => p.def.defName)
                    .ToList();

                List<Pawn> mapAnimals = OnlineManager.onlineMap.mapPawns.AllPawns
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

                foreach (byte[] compressedHuman in activityData.mapHumans)
                {
                    HumanData humanDetailsJSON = (HumanData)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in activityData.mapAnimals)
                {
                    AnimalData animalData = (AnimalData)Serializer.ConvertBytesToObject(compressedAnimal);
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

                foreach (byte[] compressedHuman in activityData.caravanHumans)
                {
                    HumanData humanDetailsJSON = (HumanData)Serializer.ConvertBytesToObject(compressedHuman);
                    Pawn human = HumanScribeManager.StringToHuman(humanDetailsJSON);
                    pawnList.Add(human);
                }

                foreach (byte[] compressedAnimal in activityData.caravanAnimals)
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

        public static List<byte[]> GetActivityHumanBytes()
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapHumans = OnlineManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsHuman(fetch))
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

        public static List<byte[]> GetActivityAnimalBytes()
        {
            if (ClientValues.isRealTimeHost)
            {
                List<Pawn> mapAnimals = OnlineManager.onlineMap.mapPawns.AllPawns
                    .FindAll(fetch => DeepScribeHelper.CheckIfThingIsAnimal(fetch))
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

        public static bool CheckIfIgnoreThingSync(Thing toCheck)
        {
            if (toCheck is Projectile) return true;
            else if (toCheck is Mote) return true;
            else return false;
        }
    }
}