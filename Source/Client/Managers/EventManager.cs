using RimWorld;
using Shared;
using Verse;


namespace GameClient
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
            EventDetailsJSON eventDetailsJSON = (EventDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

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
                Logs.Warning("Server didn't have event prices set, defaulting to 0");

                eventCosts = new int[9]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            }
        }

        public static void ShowSendEventDialog()
        {
            RT_Dialog_YesNo d1 = new RT_Dialog_YesNo($"This event will cost you {eventCosts[(int)DialogManager.inputCache[0]]} " +
                $"silver, continue?", SendEvent, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendEvent()
        {
            DialogManager.PopDialog();

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(eventCosts[(int)DialogManager.inputCache[0]]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                TransferManagerHelper.RemoveThingFromCaravan(ThingDefOf.Silver, eventCosts[(int)DialogManager.inputCache[0]]);

                EventDetailsJSON eventDetailsJSON = new EventDetailsJSON();
                eventDetailsJSON.eventStepMode = ((int)CommonEnumerators.EventStepMode.Send).ToString();
                eventDetailsJSON.fromTile = Find.AnyPlayerHomeMap.Tile.ToString();
                eventDetailsJSON.toTile = ClientValues.chosenSettlement.Tile.ToString();
                eventDetailsJSON.eventID = ((int)DialogManager.inputCache[0]).ToString();

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventDetailsJSON);
                Network.listener.EnqueuePacket(packet);

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
            Map map = Find.AnyPlayerHomeMap;

            IncidentParms parms = null;
            IncidentParms defaultParms = null;

            if (eventID == 0)
            {
                incidentDef = IncidentDefOf.RaidEnemy;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    raidArrivalMode = defaultParms.raidArrivalMode,
                    raidStrategy = defaultParms.raidStrategy,
                    customLetterLabel = "Event - Raid",
                    points = defaultParms.points,
                    faction = Faction.OfPirates,
                    target = map
                };
            }

            else if (eventID == 1)
            {
                incidentDef = IncidentDefOf.Infestation;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Infestation",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 2)
            {
                incidentDef = IncidentDefOf.MechCluster;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Cluster",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 3)
            {
                foreach (GameCondition condition in Find.World.GameConditionManager.ActiveConditions)
                {
                    if (condition.def == GameConditionDefOf.ToxicFallout) return;
                }

                incidentDef = IncidentDefOf.ToxicFallout;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Fallout",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 4)
            {
                incidentDef = IncidentDefOf.ManhunterPack;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Manhunter",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 5)
            {
                incidentDef = IncidentDefOf.WandererJoin;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Wanderer",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 6)
            {
                incidentDef = IncidentDefOf.FarmAnimalsWanderIn;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Animals",
                    points = defaultParms.points,
                    target = map
                };
            }

            else if (eventID == 7)
            {
                incidentDef = IncidentDefOf.ShipChunkDrop;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    customLetterLabel = "Event - Space Chunks",
                    points = defaultParms.points,
                    target = map,
                };

                RimworldManager.GenerateLetter("Event - Space Chunks", "Space chunks", LetterDefOf.PositiveEvent);
            }

            else if (eventID == 8)
            {
                incidentDef = IncidentDefOf.TraderCaravanArrival;
                defaultParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);

                parms = new IncidentParms
                {
                    faction = FactionValues.neutralPlayer,
                    customLetterLabel = "Event - Trader",
                    traderKind = defaultParms.traderKind,
                    points = defaultParms.points,
                    target = map
                };
            }

            incidentDef.Worker.TryExecute(parms);

            SaveManager.ForceSave();
        }

        public static void OnEventSent()
        {
            DialogManager.PopDialog();

            RimworldManager.GenerateLetter("Event sent!", "Your event has been sent and received!", 
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void OnRecoverEventSilver()
        {
            DialogManager.PopDialog();

            Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
            silverToReturn.stackCount = eventCosts[(int)DialogManager.inputCache[0]];
            TransferManagerHelper.TransferItemIntoCaravan(silverToReturn);

            DialogManager.PushNewDialog(new RT_Dialog_OK("Spent silver has been recovered"));

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }
    }
}
