using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;


namespace GameClient
{
    public static class OfflineEventManager
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
            EventData eventData = (EventData)Serializer.ConvertBytesToObject(packet.contents);

            switch (eventData.eventStepMode)
            {
                case EventStepMode.Send:
                    OnEventSent();
                    break;

                case EventStepMode.Receive:
                    OnEventReceived(eventData);
                    break;

                case EventStepMode.Recover:
                    OnRecoverEventSilver();
                    break;
            }
        }

        public static void SetEventPrices(ServerGlobalData serverGlobalData)
        {
            eventCosts = new int[]
            {
                serverGlobalData.eventValues.RaidCost,
                serverGlobalData.eventValues.InfestationCost,
                serverGlobalData.eventValues.MechClusterCost,
                serverGlobalData.eventValues.ToxicFalloutCost,
                serverGlobalData.eventValues.ManhunterCost,
                serverGlobalData.eventValues.WandererCost,
                serverGlobalData.eventValues.FarmAnimalsCost,
                serverGlobalData.eventValues.ShipChunkCost,
                serverGlobalData.eventValues.TraderCaravanCost
            };
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

            if (!RimworldManager.CheckIfHasEnoughSilverInCaravan(ClientValues.chosenCaravan, eventCosts[DialogManager.selectedScrollButton]))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver!"));
            }

            else
            {
                RimworldManager.RemoveThingFromCaravan(ThingDefOf.Silver, eventCosts[DialogManager.selectedScrollButton]);

                EventData eventData = new EventData();
                eventData.eventStepMode = EventStepMode.Send;
                eventData.fromTile = Find.AnyPlayerHomeMap.Tile;
                eventData.toTile = ClientValues.chosenSettlement.Tile;
                eventData.eventID = DialogManager.selectedScrollButton;

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.EventPacket), eventData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for event"));
            }
        }

        public static void OnEventReceived(EventData eventData)
        {
            if (ClientValues.isReadyToPlay) LoadEvent(eventData.eventID);
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
            DialogManager.PopWaitDialog();

            RimworldManager.GenerateLetter("Event sent!", "Your event has been sent!", 
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void OnRecoverEventSilver()
        {
            DialogManager.PopWaitDialog();

            Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
            silverToReturn.stackCount = eventCosts[DialogManager.selectedScrollButton];
            RimworldManager.PlaceThingIntoCaravan(silverToReturn, ClientValues.chosenCaravan);

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }
    }
}
