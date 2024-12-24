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
        public static void ParsePacket(Packet packet)
        {
            OfflineActivityData offlineVisitData = Serializer.ConvertBytesToObject<OfflineActivityData>(packet.contents);

            switch (offlineVisitData._stepMode)
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
            if (!SessionValues.actionValues.EnableOfflineActivities)
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("This feature has been disabled in this server!"));
                return;
            }

            SessionValues.ToggleOfflineActivity(activityType);

            if (activityType == OfflineActivityType.Spy)
            {
                Action r1 = delegate
                {
                    if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(SessionValues.chosenCaravan, SessionValues.actionValues.SpyCost))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                    }

                    else
                    {
                        RimworldManager.RemoveThingFromCaravan(SessionValues.chosenCaravan, ThingDefOf.Silver, SessionValues.actionValues.SpyCost);
                        SendRequest();
                    }
                };

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Spying a settlement costs {SessionValues.actionValues.SpyCost} silver, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
            else SendRequest();
        }

        private static void SendRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            OfflineActivityData data = new OfflineActivityData();
            data._stepMode = OfflineActivityStepMode.Request;
            data._targetTile = SessionValues.chosenSettlement.Tile;

            Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineActivityDeny()
        {
            if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverToReturn.stackCount = SessionValues.actionValues.SpyCost;

                RimworldManager.PlaceThingIntoCaravan(silverToReturn, SessionValues.chosenCaravan);
            }

            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("This user is currently unavailable!"));
        }

        //Executes after the action is unavailable

        private static void OnOfflineActivityUnavailable()
        {
            if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverToReturn.stackCount = SessionValues.actionValues.SpyCost;

                RimworldManager.PlaceThingIntoCaravan(silverToReturn, SessionValues.chosenCaravan);
            }

            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("This user is currently unavailable!"));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineActivityData offlineVisitData)
        {
            DialogManager.PopWaitDialog();

            Action r1 = delegate 
            {
                if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy) SaveManager.ForceSave();
                PrepareMapForOfflineActivity(offlineVisitData._mapFile); 
            };

            r1.Invoke();
        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineActivity(MapFile mapFile)
        {
            Map map = null;

            if (SessionValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                map = MapScriber.StringToMap(mapFile, false, true, true, true, true, true);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                map = MapScriber.StringToMap(mapFile, true, true, true, true, true, true, true);
            }

            else if (SessionValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                map = MapScriber.StringToMap(mapFile, false, true, false, true, false, true);
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
    }
}
