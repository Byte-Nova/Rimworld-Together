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
                    OnActivityReject(data);
                    break;

                case OnlineActivityStepMode.Unavailable:
                    OnActivityUnavailable(data);
                    break;

                case OnlineActivityStepMode.Stop:
                    OnActivityStop(data);
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
            OnlineActivityManagerHelper.SetActivityPawns();
            OnlineActivityManagerHelper.SetOtherSidePawns(data);
            OnlineActivityManagerHelper.SetOtherSidePawnsFaction();

            if (OnlineActivityManagerHelper.isHost) CameraJumper.TryJump(OnlineActivityManagerHelper.nonFactionPawns[0].Position, OnlineActivityManagerHelper.activityMap);
            else OnlineActivityManagerHelper.JoinActivityMap(data._activityType);

            Logger.Warning($"My pawns > {OnlineActivityManagerHelper.factionPawns.Length}");
            foreach(Pawn pawn in OnlineActivityManagerHelper.factionPawns) Logger.Warning(pawn.def.defName);

            Logger.Warning($"Other pawns > {OnlineActivityManagerHelper.nonFactionPawns.Length}");
            foreach(Pawn pawn in OnlineActivityManagerHelper.nonFactionPawns) Logger.Warning(pawn.def.defName);

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

        public static Map activityMap = new Map();

        public static Pawn[] factionPawns = new Pawn[0];

        public static Pawn[] nonFactionPawns = new Pawn[0];

        public static void SetActivityHost(OnlineActivityData data)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data._toTile && fetch.Faction == Faction.OfPlayer) != null) isHost = true;
            else isHost = false;
        }

        public static void SetActivityPawns()
        {
            if (isHost) factionPawns = activityMap.mapPawns.AllPawns.ToArray();
            else factionPawns = SessionValues.chosenCaravan.PawnsListForReading.ToArray();
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

            nonFactionPawns = toSet.ToArray();
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
}
