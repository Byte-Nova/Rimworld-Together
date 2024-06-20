using RimWorld.Planet;
using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            OfflineActivityData offlineVisitData = (OfflineActivityData)Serializer.ConvertBytesToObject(packet.contents);

            switch (offlineVisitData.activityStepMode)
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
            ClientValues.ToggleOfflineFunction(activityType);

            if (activityType == OfflineActivityType.Spy)
            {
                Action r1 = delegate
                {
                    if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(ClientValues.chosenCaravan, spyCost))
                    {
                        DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
                    }

                    else
                    {
                        TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, spyCost);
                        SendRequest();
                    }
                };

                RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"Spying a settlement costs {spyCost} silver, continue?", r1, null);
                DialogManager.PushNewDialog(d1);
            }
            else SendRequest();
        }

        private static void SendRequest()
        {
            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for map"));

            OfflineActivityData data = new OfflineActivityData();
            data.activityStepMode = OfflineActivityStepMode.Request;
            data.targetTile = ClientValues.chosenSettlement.Tile;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineActivityPacket), data);
            Network.listener.EnqueuePacket(packet);
        }

        //Executes when offline visit is denied

        private static void OnOfflineActivityDeny()
        {
            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player must not be connected!"));
        }

        //Executes after the action is unavailable

        private static void OnOfflineActivityUnavailable()
        {
            if (ClientValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
                silverToReturn.stackCount = spyCost;

                TransferManagerHelper.TransferItemIntoCaravan(silverToReturn);
            }

            DialogManager.PopWaitDialog();

            DialogManager.PushNewDialog(new RT_Dialog_Error("This user is currently unavailable!"));
        }

        //Executes when offline visit is accepted

        private static void OnRequestAccepted(OfflineActivityData offlineVisitData)
        {
            DialogManager.PopWaitDialog();

            MapFileData mapFileData = (MapFileData)Serializer.ConvertBytesToObject(offlineVisitData.mapData);
            MapData mapData = (MapData)Serializer.ConvertBytesToObject(mapFileData.mapData);

            Action r1 = delegate 
            {
                if (ClientValues.latestOfflineActivity == OfflineActivityType.Spy) SaveManager.ForceSave();
                PrepareMapForOfflineActivity(mapData); 
            };

            if (ModManager.CheckIfMapHasConflictingMods(mapData))
            {
                DialogManager.PushNewDialog(new RT_Dialog_YesNo("Map received but contains unknown mod data, continue?", r1, null));
            }
            else r1.Invoke();
        }

        //Prepares a map for the offline visit feature from a request

        private static void PrepareMapForOfflineActivity(MapData mapData)
        {
            Map map = null;

            if (ClientValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                map = MapScribeManager.StringToMap(mapData, false, true, true, true, true, true);
            }

            else if (ClientValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                map = MapScribeManager.StringToMap(mapData, true, true, true, true, true, true, true);
            }

            else if (ClientValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                map = MapScribeManager.StringToMap(mapData, false, true, false, true, false, true);
            }

            HandleMapFactions(map);

            if (ClientValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
                    CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
            }

            else if (ClientValues.latestOfflineActivity == OfflineActivityType.Raid)
            {
                SettlementUtility.Attack(ClientValues.chosenCaravan, ClientValues.chosenSettlement);
            }

            else if (ClientValues.latestOfflineActivity == OfflineActivityType.Spy)
            {
                CaravanEnterMapUtility.Enter(ClientValues.chosenCaravan, map, CaravanEnterMode.Edge,
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
                    if (ClientValues.latestOfflineActivity == OfflineActivityType.Visit) { pawn.SetFaction(FactionValues.allyPlayer); }
                    else if (ClientValues.latestOfflineActivity == OfflineActivityType.Raid) { pawn.SetFaction(FactionValues.enemyPlayer); }
                }
            }

            foreach (Thing thing in map.listerThings.AllThings.ToArray())
            {
                if (thing.Faction == FactionValues.neutralPlayer)
                {
                    if (ClientValues.latestOfflineActivity == OfflineActivityType.Visit) { thing.SetFaction(FactionValues.allyPlayer); }
                    else if (ClientValues.latestOfflineActivity == OfflineActivityType.Raid) { thing.SetFaction(FactionValues.enemyPlayer); }
                }
            }
        }

        //Prepares the map lord of a desired map for the offline visit

        private static void PrepareMapLord(Map map)
        {
            Thing toFocusOn;
            IntVec3 deployPlace;

            if (ClientValues.latestOfflineActivity == OfflineActivityType.Visit)
            {
                deployPlace = map.Center;
                toFocusOn = map.listerThings.AllThings.Find(x => x.def.defName == "RTChillSpot");
                if (toFocusOn != null) deployPlace = toFocusOn.Position;

                Pawn[] lordPawns = map.mapPawns.AllPawns.ToList().FindAll(fetch => fetch.Faction == FactionValues.allyPlayer).ToArray();
                LordJob_DefendBase job = new LordJob_DefendBase(FactionValues.allyPlayer, deployPlace, false);
                LordMaker.MakeNewLord(FactionValues.allyPlayer, job, map, lordPawns);
            }

            else if (ClientValues.latestOfflineActivity == OfflineActivityType.Raid)
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
