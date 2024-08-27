using RimWorld.Planet;
using RimWorld;
using Shared;
using System;
using System.Linq;
using Verse.AI.Group;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class OfflineActivityManager
    {
        public static int spyCost;

        public static void ParseOfflineActivityPacket(Packet packet)
        {
            OfflineActivityData offlineVisitData = Serializer.ConvertBytesToObject<OfflineActivityData>(packet.contents);

            switch (offlineVisitData.stepMode)
            {
                case OfflineActivityStepMode.Request:
                    OnRequestAccepted(offlineVisitData);
                    break;

                case OfflineActivityStepMode.Deny:
                    OnOfflineActivityDeny();
                    break;

                case OfflineActivityStepMode.Unavailable:
                    OnOfflineActivityUnavailable();
                    break;
            }
        }

        //Requests a raid to the server

        public static void RequestOfflineActivity(OfflineActivityType activityType)
        {
            SessionValues.ToggleOfflineFunction(activityType);

            if (activityType == OfflineActivityType.Spy)
            {
                Action r1 = delegate
                {
                    if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.chosenCaravan, spyCost))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("RTNotEnoughSilver".Translate()));
                    }

                    else
                    {
                        RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, spyCost, SessionValues.chosenCaravan);
                        SendRequest();
                    }
                };

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo("RTSpyingCost".Translate(spyCost), r1, null);
                DialogManager.PushNewDialog(d1);
            }
            else SendRequest();
        }

        private static void SendRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("RTMapWait".Translate()));

            OfflineActivityData data = new OfflineActivityData();
            data.stepMode = OfflineActivityStepMode.Request;
            data.targetTile = SessionValues.chosenSettlement.Tile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OfflineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineActivityDeny()
        {
            if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverToReturn.stackCount = spyCost;

                RimworldManager.PlaceThingIntoCaravan(silverToReturn, SessionValues.chosenCaravan);
            }

            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerNotAvailable".Translate()));
        }

        //Executes after the action is unavailable

        private static void OnOfflineActivityUnavailable()
        {
            if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverToReturn.stackCount = spyCost;

                RimworldManager.PlaceThingIntoCaravan(silverToReturn, SessionValues.chosenCaravan);
            }

            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("RTPlayerNotAvailable".Translate()));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineActivityData offlineVisitData)
        {
            DialogManager.PopWaitDialog();

            MapData mapData = offlineVisitData.mapData;

            Action r1 = delegate 
            {
                if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy) SaveManager.ForceSave();
                PrepareMapForOfflineActivity(mapData); 
            };

            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("RTMapUnknownModData".Translate(), r1, null));
            }
            else r1.Invoke();
        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineActivity(MapData mapData)
        {
            Map map = null;

            if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                map = MapScribeManager.StringToMap(mapData, false, true, true, true, true, true);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                map = MapScribeManager.StringToMap(mapData, true, true, true, true, true, true, true);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                map = MapScribeManager.StringToMap(mapData, false, true, false, true, false, true);
            }

            HandleMapFactions(map);

            if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                CaravanEnterMapUtility.Enter(SessionValues.chosenCaravan, map, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                SettlementUtility.Attack(SessionValues.chosenCaravan, SessionValues.chosenSettlement);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                CaravanEnterMapUtility.Enter(SessionValues.chosenCaravan, map, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
            }

            PrepareMapLord(map);
        }

        //Handles the factions of a desired map for the offline visit

        private static void HandleMapFactions(Map map)
        {
            foreach (Pawn pawn in map.mapPawns.AllPawns.ToArray())
            {
                if (pawn.Faction == FactionValues.neutralPlayer)
                {
                    if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit) { pawn.SetFaction(FactionValues.allyPlayer); }
                    else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid) { pawn.SetFaction(FactionValues.enemyPlayer); }
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == FactionValues.neutralPlayer)
                {
                    if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit) { thing.SetFaction(FactionValues.allyPlayer); }
                    else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid) { thing.SetFaction(FactionValues.enemyPlayer); }
                }
            }
        }

        //Prepares the map lord of a desired map for the offline visit

        private static void PrepareMapLord(Map map)
        {
            Thing toFocusOn;
            IntVec3 deployPlace;

            if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                deployPlace = map.Center;
                toFocusOn = map.listerThings.AllThings.Find(x => x.def.defName == "RTChillSpot");
                if (toFocusOn != null) deployPlace = toFocusOn.Position;

                Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == FactionValues.allyPlayer).ToArray();
                LordJob_DefendBase job = new LordJob_DefendBase(FactionValues.allyPlayer, deployPlace, false);
                LordMaker.MakeNewLord(FactionValues.allyPlayer, job, map, lordPawns);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                deployPlace = map.Center;
                toFocusOn = map.listerThings.AllThings.Find(x => x.def.defName == "RTDefenseSpot");
                if (toFocusOn != null) deployPlace = toFocusOn.Position;

                Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == FactionValues.enemyPlayer).ToArray();
                LordJob_DefendBase job = new LordJob_DefendBase(FactionValues.enemyPlayer, deployPlace, true);
                LordMaker.MakeNewLord(FactionValues.enemyPlayer, job, map, lordPawns);
            }
        }

        //TODO
        //Remove from here

        public static void SetSpyCost(ServerGlobalData serverGlobalData)
        {
            try { spyCost = serverGlobalData.actionValues.SpyCost; }
            catch
            {
                Logger.Warning("Server didn't have spy cost set, defaulting to 0");

                spyCost = 0;
            }
        }
    }
}
