using System;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameClient
{
    public static class OnlineActivityManager
    {
        public static Map activityMap = new Map();

        public static List<Thing> activityMapThings = new List<Thing>();

        public static List<Pawn> factionPawns = new List<Pawn>();

        public static List<Pawn> nonFactionPawns = new List<Pawn>();

        public static void ParsePacket(Packet packet)
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

                case OnlineActivityStepMode.Stop:
                    OnActivityStop();
                    break;

                case OnlineActivityStepMode.Jobs:
                    //Nothing yet
                    break;

                case OnlineActivityStepMode.Create:
                    OnlineActivityManagerOrders.ReceiveCreationOrder(data);
                    break;

                case OnlineActivityStepMode.Destroy:
                    OnlineActivityManagerOrders.ReceiveDestructionOrder(data);
                    break;

                case OnlineActivityStepMode.Damage:
                    OnlineActivityManagerOrders.ReceiveDamageOrder(data);
                    break;

                case OnlineActivityStepMode.Hediff:
                    OnlineActivityManagerOrders.ReceiveHediffOrder(data);
                    break;

                case OnlineActivityStepMode.GameCondition:
                    OnlineActivityManagerOrders.ReceiveGameConditionOrder(data);
                    break;

                case OnlineActivityStepMode.Weather:
                    OnlineActivityManagerOrders.ReceiveWeatherOrder(data);
                    break;

                case OnlineActivityStepMode.TimeSpeed:
                    OnlineActivityManagerOrders.ReceiveTimeSpeedOrder(data);
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
            SessionValues.ToggleOnlineActivity(data._activityType);
            OnlineActivityManagerHelper.SetActivityHost(data);
            OnlineActivityManagerHelper.SetActivityMap(data);
            OnlineActivityManagerHelper.SetActivityMapThings();
            OnlineActivityManagerHelper.SetFactionPawnsForActivity();
            OnlineActivityManagerHelper.SetNonFactionPawnsForActivity(data);

            if (SessionValues.isActivityHost) CameraJumper.TryJump(nonFactionPawns[0].Position, activityMap);
            else OnlineActivityManagerHelper.JoinActivityMap(data._activityType);

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
            DialogManager.PushNewDialog(new RT_Dialog_OK($"Should cancel {SessionValues.isActivityHost}"));
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

        public static Thing GetThingFromHash(string hash)
        {
            return OnlineActivityManager.activityMapThings.First(fetch => ExtensionManager.GetThingHash(fetch) == hash);
        }

        public static Pawn GetPawnFromHash(string hash, bool isFromFaction)
        {
            if (isFromFaction) return OnlineActivityManager.factionPawns.First(fetch => ExtensionManager.GetThingHash(fetch) == hash);
            else return OnlineActivityManager.nonFactionPawns.First(fetch => ExtensionManager.GetThingHash(fetch) == hash);
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

    public static class OnlineActivityManagerJobs
    {
        private static readonly float taskDelayMS = 1000f;

        public static async Task StartJobsTicker()
        {
            while (SessionValues.currentRealTimeEvent != OnlineActivityType.None)
            {
                try { JobsTick(); }
                catch (Exception e) { Logger.Error($"Jobs tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMilliseconds(taskDelayMS));
            }
        }

        public static void JobsTick()
        {
            PawnOrderData pawnOrderData = new PawnOrderData();
            List<PawnOrderComponent> pawnOrders = new List<PawnOrderComponent>();
            foreach (Pawn pawn in OnlineActivityManager.factionPawns.ToArray()) pawnOrders.Add(GetPawnJob(pawn));

            OnlineActivityData onlineActivityData = new OnlineActivityData();
            onlineActivityData._stepMode = OnlineActivityStepMode.Jobs;
            onlineActivityData._pawnOrder = pawnOrderData;

            Packet packet = Packet.CreatePacketFromObject(nameof(OnlineActivityManager), onlineActivityData);
            Network.listener.EnqueuePacket(packet);
        }

        public static PawnOrderComponent GetPawnJob(Pawn pawn)
        {
            return new PawnOrderComponent();
        }
    }

    public static class OnlineActivityManagerOrders
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
            destructionOrder._thingHash = ExtensionManager.GetThingHash(thing);
            
            return destructionOrder;
        }

        public static DamageOrderData CreateDamageOrder(DamageInfo damageInfo, Thing affectedThing)
        {
            DamageOrderData damageOrder = new DamageOrderData();
            damageOrder._defName = damageInfo.Def.defName;
            damageOrder._damageAmount = damageInfo.Amount;
            damageOrder._ignoreArmor = damageInfo.IgnoreArmor;
            damageOrder._armorPenetration = damageInfo.ArmorPenetrationInt;
            damageOrder.targetHash = ExtensionManager.GetThingHash(affectedThing);
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
                hediffOrder._hediffTargetHash = ExtensionManager.GetThingHash(pawn);
            }

            else
            {
                hediffOrder._pawnFaction = OnlineActivityTargetFaction.Faction;
                hediffOrder._hediffTargetHash = ExtensionManager.GetThingHash(pawn);
            }

            hediffOrder._hediffDefName = hediff.def.defName;
            if (hediff.Part != null) hediffOrder._hediffPartDefName = hediff.Part.def.defName;
            if (hediff.sourceDef != null) hediffOrder._hediffWeaponDefName = hediff.sourceDef.defName;
            hediffOrder._hediffSeverity = hediff.Severity;
            hediffOrder._hediffPermanent = hediff.IsPermanent();

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
            Thing toDestroy = OnlineActivityManagerHelper.GetThingFromHash(data._destructionOrder._thingHash);
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
                Thing toApplyTo = OnlineActivityManagerHelper.GetThingFromHash(data._damageOrder.targetHash);
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
                if (data._hediffOrder._pawnFaction == OnlineActivityTargetFaction.Faction) toTarget = OnlineActivityManagerHelper.GetPawnFromHash(data._hediffOrder._hediffTargetHash, true);
                else toTarget = OnlineActivityManagerHelper.GetPawnFromHash(data._hediffOrder._hediffTargetHash, false);

                // If we receive a hash that doesn't exist or we are host we ignore it
                if (toTarget != null && !SessionValues.isActivityHost)
                {
                    OnlineActivityQueues.SetThingQueue(toTarget);

                    BodyPartRecord bodyPartRecord = toTarget.RaceProps.body.AllParts.FirstOrDefault(fetch => fetch.def.defName == data._hediffOrder._hediffPartDefName);

                    if (data._hediffOrder._applyMode == OnlineActivityApplyMode.Add)
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder._hediffDefName);
                        Hediff toMake = HediffMaker.MakeHediff(hediffDef, toTarget, bodyPartRecord);
                        
                        if (data._hediffOrder._hediffWeaponDefName != null)
                        {
                            ThingDef source = DefDatabase<ThingDef>.AllDefs.First(fetch => fetch.defName == data._hediffOrder._hediffWeaponDefName);
                            toMake.sourceDef = source;
                            toMake.sourceLabel = source.label;
                        }

                        toMake.Severity = data._hediffOrder._hediffSeverity;

                        if (data._hediffOrder._hediffPermanent)
                        {
                            HediffComp_GetsPermanent hediffComp = toMake.TryGetComp<HediffComp_GetsPermanent>();
                            hediffComp.IsPermanent = true;
                        }

                        toTarget.health.AddHediff(toMake, bodyPartRecord);
                    }

                    else if (data._hediffOrder._applyMode == OnlineActivityApplyMode.Remove)
                    {
                        Hediff hediff = toTarget.health.hediffSet.hediffs.First(fetch => fetch.def.defName == data._hediffOrder._hediffDefName &&
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
