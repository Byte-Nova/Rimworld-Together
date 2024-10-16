using System;
using Verse;
using Shared;
using static Shared.CommonEnumerators;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace GameClient
{
    public static class OnlineActivityManager
    {
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
                    OnActivityReject(data);
                    break;

                case OnlineActivityStepMode.Unavailable:
                    OnActivityUnavailable(data);
                    break;

                case OnlineActivityStepMode.Stop:
                    OnActivityStop(data);
                    break;

                case OnlineActivityStepMode.Create:
                    OnlineManagerOrders.ReceiveCreationOrder(data);
                    break;

                case OnlineActivityStepMode.Destroy:
                    OnlineManagerOrders.ReceiveDestructionOrder(data);
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
            OnlineActivityManagerHelper.SetActivityMapThings(data);
            OnlineActivityManagerHelper.SetActivityPawns();
            OnlineActivityManagerHelper.SetOtherSidePawns(data);
            OnlineActivityManagerHelper.SetOtherSidePawnsFaction();
            OnlineActivityManagerHelper.SetActivityReady(true);

            if (OnlineActivityManagerHelper.isHost) CameraJumper.TryJump(OnlineActivityManagerHelper.nonFactionPawns[0].Position, OnlineActivityManagerHelper.activityMap);
            else OnlineActivityManagerHelper.JoinActivityMap(data._activityType);

            //Logger.Warning($"My pawns > {OnlineActivityManagerHelper.factionPawns.Count}");
            //(Pawn pawn in OnlineActivityManagerHelper.factionPawns) Logger.Warning(pawn.def.defName);

            //Logger.Warning($"Other pawns > {OnlineActivityManagerHelper.nonFactionPawns.Count}");
            //foreach(Pawn pawn in OnlineActivityManagerHelper.nonFactionPawns) Logger.Warning(pawn.def.defName);

            Logger.Warning($"Map things > {OnlineActivityManagerHelper.activityMapThings.Count}");
            //foreach(ThingDataFile thingData in OnlineActivityManagerHelper.activityMapThings) Logger.Warning(thingData.Hash);

            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK($"Should start {OnlineActivityManagerHelper.isHost}"));
        }

        private static void OnActivityReject(OnlineActivityData data)
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_OK($"Should cancel {OnlineActivityManagerHelper.isHost}"));
        }

        private static void OnActivityUnavailable(OnlineActivityData data)
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error($"This user is currently unavailable! {OnlineActivityManagerHelper.isHost}"));
        }

        private static void OnActivityStop(OnlineActivityData data)
        {
            SessionValues.ToggleOnlineActivity(OnlineActivityType.None);

            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error($"Activity has ended! {OnlineActivityManagerHelper.isHost}"));
        }
    }

    public static class OnlineActivityManagerHelper
    {
        public static bool isHost;

        public static bool isActivityReady;

        public static Map activityMap = new Map();

        public static Dictionary<string, Thing> activityMapThings = new Dictionary<string, Thing>();

        public static List<Pawn> factionPawns = new List<Pawn>();

        public static List<Pawn> nonFactionPawns = new List<Pawn>();

        // Queues

        public static string queuedHash;

        public static Thing queuedThing;

        public static void SetThingQueue(Thing toSetTo) { queuedThing = toSetTo; }

        public static void SetHashQueue(string toSetTo) { queuedHash = toSetTo; }

        public static void SetActivityReady(bool value) { isActivityReady = value; }

        // Stuff

        public static void AddThingToMap(Thing toAdd, string thingHash) 
        { 
            activityMapThings.Add(thingHash, toAdd);
            SetThingQueue(null);
            SetHashQueue(null);
        }

        public static void RemoveThingFromMap(Thing toRemove)
        {
            KeyValuePair<string, Thing> pair = activityMapThings.First(fetch => fetch.Value == toRemove);
            activityMapThings.Remove(pair.Key);
            SetThingQueue(null);
            SetHashQueue(null);
        }

        public static void SetActivityHost(OnlineActivityData data)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer) != null) isHost = true;
            else isHost = false;
        }

        public static void SetActivityPawns()
        {
            if (isHost) factionPawns = activityMap.mapPawns.AllPawns.ToList();
            else factionPawns = SessionValues.chosenCaravan.PawnsListForReading.ToList();
        }

        public static void SetOtherSidePawns(OnlineActivityData data)
        {
            List<Pawn> toSet = new List<Pawn>();

            if (isHost)
            {
                foreach (HumanFile human in data._guestHumans)
                {
                    Pawn toSpawn = HumanScribeManager.StringToHuman(human);
                    toSpawn.Position = activityMap.Center;

                    RimworldManager.PlaceThingIntoMap(toSpawn, activityMap);
                    toSet.Add(toSpawn);
                }

                foreach (AnimalFile animal in data._guestAnimals)
                {
                    Pawn toSpawn = AnimalScribeManager.StringToAnimal(animal);
                    toSpawn.Position = activityMap.Center;

                    RimworldManager.PlaceThingIntoMap(toSpawn, activityMap);
                    toSet.Add(toSpawn);
                }
            }

            else
            {
                foreach (Pawn pawn in activityMap.mapPawns.AllPawns.Where(fetch => fetch.Faction != Faction.OfPlayer))
                {
                    toSet.Add(pawn);
                }
            }

            nonFactionPawns = toSet.ToList();
        }

        public static void SetOtherSidePawnsFaction()
        {
            foreach (Pawn pawn in nonFactionPawns)
            {
                if (SessionValues.currentRealTimeEvent == OnlineActivityType.Visit) pawn.SetFactionDirect(FactionValues.allyPlayer);
                else pawn.SetFactionDirect(FactionValues.enemyPlayer);
            }
        }

        public static void SetActivityMap(OnlineActivityData data)
        {
            if (isHost) activityMap = Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer).Map;
            else activityMap = MapScribeManager.StringToMap(data._mapFile, true, true, true, true, true, true);
        }

        public static void SetActivityMapThings(OnlineActivityData data)
        {
            List<ThingDataFile> thingDatas = new List<ThingDataFile>();
            thingDatas.AddRange(data._mapFile.FactionThings);
            thingDatas.AddRange(data._mapFile.NonFactionThings);

            foreach(ThingDataFile thingData in thingDatas)
            {
                Thing toGet = GetThingFromData(thingData);
                activityMapThings.Add(thingData.Hash, toGet);
            }
        }

        public static Thing GetThingFromData(ThingDataFile data)
        {
            IntVec3 suggestedPosition = new IntVec3(data.TransformComponent.Position[0], data.TransformComponent.Position[1], data.TransformComponent.Position[2]);
            Rot4 suggestedRotation = new Rot4(data.TransformComponent.Rotation);

            return activityMap.listerThings.AllThings.Find(fetch => 
                fetch.Position == suggestedPosition && 
                fetch.Rotation == suggestedRotation && 
                fetch.def.defName == data.DefName);
        }

        public static string GetHashFromThing(Thing thing)
        {
            foreach(KeyValuePair<string, Thing> pair in activityMapThings)
            {
                if (pair.Value == thing) return pair.Key;
            }

            return null;
        }

        public static Thing GetThingFromHash(string hash)
        {
            foreach(KeyValuePair<string, Thing> pair in activityMapThings)
            {
                if (pair.Key == hash) return pair.Value;
            }

            return null;
        }

        public static bool CheckIfIgnoreThingSync(Thing toCheck)
        {
            if (toCheck is Projectile) return true;
            else if (toCheck is Mote) return true;
            else return false;
        }

        public static bool CheckIfShouldPatch(Thing toPatch, bool checkFactionPawns, bool checkNonFactionPawns, bool checkMapThings)
        {
            if (checkFactionPawns && factionPawns.Contains(toPatch)) return true;
            else if (checkNonFactionPawns && nonFactionPawns.Contains(toPatch)) return true;
            else if (checkMapThings && activityMapThings.Values.Contains(toPatch)) return true;
            else return false;
        }

        public static bool CheckInverseIfShouldPatch(Thing toPatch, bool checkFactionPawns, bool checkNonFactionPawns, bool checkMapThings)
        {
            bool shouldPatch = true;
            if (checkFactionPawns && factionPawns.Contains(toPatch)) shouldPatch = false;
            else if (checkNonFactionPawns && nonFactionPawns.Contains(toPatch)) shouldPatch = false;
            else if (checkMapThings && activityMapThings.Values.Contains(toPatch)) shouldPatch = false;
            
            return shouldPatch;
        }

        public static void JoinActivityMap(OnlineActivityType activityType)
        {
            if (activityType == OnlineActivityType.Visit)
            {
                CaravanEnterMapUtility.Enter(SessionValues.chosenCaravan, activityMap, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: false);
            }

            else if (activityType == OnlineActivityType.Raid)
            {
                SettlementUtility.Attack(SessionValues.chosenCaravan, SessionValues.chosenSettlement);
            }

            CameraJumper.TryJump(factionPawns[0].Position, activityMap);
        }

        public static HumanFile[] GetActivityHumans()
        {
            List<HumanFile> toGet = new List<HumanFile>();

            if (isHost)
            {
                foreach (Pawn pawn in activityMap.mapPawns.AllPawns.Where(fetch => fetch.Faction == Faction.OfPlayer && DeepScribeHelper.CheckIfThingIsHuman(fetch)))
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

            if (isHost)
            {
                foreach (Pawn pawn in activityMap.mapPawns.AllPawns.Where(fetch => fetch.Faction == Faction.OfPlayer && DeepScribeHelper.CheckIfThingIsAnimal(fetch)))
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

    public static class OnlineManagerOrders
    {
        public static CreationOrderData CreateCreationOrder(Thing thing)
        {
            CreationOrderData creationOrder = new CreationOrderData();

            if (DeepScribeHelper.CheckIfThingIsHuman(thing)) creationOrder._creationType = CreationType.Human;
            else if (DeepScribeHelper.CheckIfThingIsAnimal(thing)) creationOrder._creationType = CreationType.Animal;
            else creationOrder._creationType = CreationType.Thing;

            //Modify position based on center cell because RimWorld doesn't store it by default
            thing.Position = thing.OccupiedRect().CenterCell;
            creationOrder._thingHash = Hasher.GetHashFromString(thing.ThingID);

            if (creationOrder._creationType == CreationType.Human) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString((Pawn)thing));
            else if (creationOrder._creationType == CreationType.Animal) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(AnimalScribeManager.AnimalToString((Pawn)thing));
            else if (creationOrder._creationType == CreationType.Thing) creationOrder._dataToCreate = Serializer.ConvertObjectToBytes(ThingScribeManager.ItemToString(thing, thing.stackCount));

            return creationOrder;
        }

        public static DestructionOrderData CreateDestructionOrder(Thing thing)
        {
            DestructionOrderData destructionOrder = new DestructionOrderData();
            destructionOrder._thingHash = OnlineActivityManagerHelper.GetHashFromThing(thing);
            
            return destructionOrder;
        }

        public static void ReceiveCreationOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;
            if (!OnlineActivityManagerHelper.isActivityReady) return;

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
            if (toCreate != null && !OnlineActivityManagerHelper.isHost)
            {
                OnlineActivityManagerHelper.SetThingQueue(toCreate);
                OnlineActivityManagerHelper.SetHashQueue(data._creationOrder._thingHash);
                RimworldManager.PlaceThingIntoMap(toCreate, OnlineActivityManagerHelper.activityMap, ThingPlaceMode.Direct, false);
            }
        }

        public static void ReceiveDestructionOrder(OnlineActivityData data)
        {
            if (SessionValues.currentRealTimeEvent == OnlineActivityType.None) return;
            if (!OnlineActivityManagerHelper.isActivityReady) return;

            // If we receive a hash that doesn't exist or we are host we ignore it
            Thing toDestroy = OnlineActivityManagerHelper.GetThingFromHash(data._destructionOrder._thingHash);
            if (toDestroy != null && !OnlineActivityManagerHelper.isHost)
            {
                OnlineActivityManagerHelper.SetThingQueue(toDestroy);
                toDestroy.Destroy(DestroyMode.Vanish);
            }
        }

    }
}
