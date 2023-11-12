using RimWorld;
using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;
using Verse;


namespace RimworldTogether.GameClient.Managers.Actions
{
    public static class EventManager
    {
        

        public static string[] eventNames = new string[]
        {
            "Raid",
            "Infestation",
            "Mech Cluster",
            "Toxic Fallout",
            "Manhunter",
            "Wanderer",
            "Farm Animals",
            "Ship Chunks",
            "Trader Caravan"
        };

        public static int[] eventCosts;

        public static void ParseEventPacket(Packet packet)
        {
            EventDetailsJSON eventDetailsJSON = (EventDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(eventDetailsJSON.eventStepMode))
            {
                case (int)CommonEnumerators.EventStepMode.Send:
                    OnEventSent();
                    break;

                case (int)CommonEnumerators.EventStepMode.Receive:
                    OnEventReceived(eventDetailsJSON);
                    break;

                case (int)CommonEnumerators.EventStepMode.Recover:
                    OnRecoverEventSilver();
                    break;
            }
        }

        public static void SetEventPrices(ServerOverallJSON serverOverallJSON)
        {
            try
            {
                eventCosts = new int[9]
                {
                    int.Parse(serverOverallJSON.RaidCost),
                    int.Parse(serverOverallJSON.InfestationCost),
                    int.Parse(serverOverallJSON.MechClusterCost),
                    int.Parse(serverOverallJSON.ToxicFalloutCost),
                    int.Parse(serverOverallJSON.ManhunterCost),
                    int.Parse(serverOverallJSON.WandererCost),
                    int.Parse(serverOverallJSON.FarmAnimalsCost),
                    int.Parse(serverOverallJSON.ShipChunkCost),
                    int.Parse(serverOverallJSON.TraderCaravanCost)
                };
            }

            catch 
            { 
                Log.Warning("Server didn't have event prices set, defaulting to 0");

                eventCosts = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void ShowSendEventDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This event will cost you {eventCosts[DialogManager.selectedScrollButton]} " +
                $"silver, continue?", SendEvent, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendEvent()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(eventCosts[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, eventCosts[DialogManager.selectedScrollButton]);

                EventDetailsJSON eventDetailsJSON = new EventDetailsJSON();
                eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Send).ToString();
                eventDetailsJSON.fromTile = Find.AnyPlayerHomeMap.Tile.ToString();
                eventDetailsJSON.toTile = ClientValues.chosenSettlement.Tile.ToString();
                eventDetailsJSON.eventID = DialogManager.selectedScrollButton.ToString();

                Packet packet = Packet.CreatePacketFromJSON("EventPacket", eventDetailsJSON);
                Network.Network.serverListener.SendData(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for event"));
            }
        }

        public static void OnEventReceived(EventDetailsJSON eventDetailsJSON)
        {
            if (ClientValues.isReadyToPlay) LoadEvent(int.Parse(eventDetailsJSON.eventID));
        }

        public static void LoadEvent(int eventID)
        {
            IncidentDef incidentDef = null;
            IncidentParms parms = null;
            Map map = Find.AnyPlayerHomeMap;

            if (eventID == 0)
            {
                incidentDef = IncidentDefOf.RaidEnemy;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Raid",
                    target = map,
                    points = defaultParms.points,
                    faction = Faction.OfPirates,
                    raidStrategy = defaultParms.raidStrategy,
                    raidArrivalMode = defaultParms.raidArrivalMode,
                };
            }

            else if (eventID == 1)
            {
                incidentDef = IncidentDefOf.Infestation;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Infestation",
                    target = Find.AnyPlayerHomeMap,
                    points = defaultParms.points,
                };
            }

            else if (eventID == 2)
            {
                incidentDef = IncidentDefOf.MechCluster;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Cluster",
                    target = map,
                    points = defaultParms.points
                };
            }

            else if (eventID == 3)
            {
                foreach (GameCondition condition in Find.World.GameConditionManager.ActiveConditions)
                {
                    if (condition.def == GameConditionDefOf.ToxicFallout) return;
                }

                incidentDef = IncidentDefOf.ToxicFallout;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Fallout",
                    target = map,
                    points = defaultParms.points
                };
            }

            else if (eventID == 4)
            {
                incidentDef = IncidentDefOf.ManhunterPack;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Manhunter",
                    target = Find.AnyPlayerHomeMap,
                    points = defaultParms.points
                };
            }

            else if (eventID == 5)
            {
                incidentDef = IncidentDefOf.WandererJoin;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Wanderer",
                    target = map,
                    points = defaultParms.points
                };
            }

            else if (eventID == 6)
            {
                incidentDef = IncidentDefOf.FarmAnimalsWanderIn;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Animals",
                    target = Find.AnyPlayerHomeMap,
                    points = defaultParms.points
                };
            }

            else if (eventID == 7)
            {
                incidentDef = IncidentDefOf.ShipChunkDrop;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    target = map,
                    points = defaultParms.points
                };

                LetterManager.GenerateLetter("Event - Space Chunks", "Space chunks", LetterDefOf.PositiveEvent);
            }

            else if (eventID == 8)
            {
                incidentDef = IncidentDefOf.TraderCaravanArrival;
                IncidentParms defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.AnyPlayerHomeMap);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Trader",
                    target = map,
                    points = defaultParms.points,
                    faction = FactionValues.neutralPlayer,
                    traderKind = defaultParms.traderKind
                };
            }

            incidentDef.Worker.TryExecute(parms);

            SaveManager.ForceSave();
        }

        public static void OnEventSent()
        {
            DialogManager.PopWaitDialog();

            LetterManager.GenerateLetter("Event sent!", "Your event has been sent and received!", 
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void OnRecoverEventSilver()
        {
            DialogManager.PopWaitDialog();

            TransferManager.SendSilverToCaravan(eventCosts[DialogManager.selectedScrollButton]);

            DialogManager.PushNewDialog(new RT_Dialog_OK("Spent silver has been recovered"));

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }
    }
}
