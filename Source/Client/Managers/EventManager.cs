using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class EventManager
    {
        public static void ParsePacket(Packet packet)
        {
            EventData eventData = Serializer.ConvertBytesToObject<EventData>(packet.contents);

            switch (eventData._stepMode)
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

        public static void ShowEventMenu()
        {
            List<string> eventNames = new List<string>();

            foreach (EventFile eventFile in EventManagerHelper.availableEvents) eventNames.Add(eventFile.Name);

            Action a1 = delegate
            {
                RT_Dialog_YesNo d2 = new RT_Dialog_YesNo($"This event will cost you {EventManagerHelper.availableEvents[DialogManager.selectedScrollButton].Cost} " +
                    $"silver, continue?", SendEvent, null);

                DialogManager.PushNewDialog(d2);
            };

            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Event Selector", "Choose the even you want to send",
                eventNames.ToArray(), a1.Invoke, null);

            DialogManager.PushNewDialog(d1);
        }

        public static void SendEvent()
        {
            DialogManager.PopDialog(DialogManager.dialogScrollButtons);

            //TODO
            //MAKE IT SO ALL MAPS ARE ACCOUNTED FOR
            Map toGetSilverFrom = Find.AnyPlayerHomeMap;

            if (!RimworldManager.CheckIfHasEnoughSilverInMap(toGetSilverFrom, EventManagerHelper.availableEvents[DialogManager.selectedScrollButton].Cost))
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("You do not have enough silver for this action!"));
            }

            else
            {
                RimworldManager.RemoveThingFromSettlement(toGetSilverFrom, ThingDefOf.Silver, EventManagerHelper.availableEvents[DialogManager.selectedScrollButton].Cost);

                EventData eventData = new EventData();
                eventData._stepMode = EventStepMode.Send;
                eventData._fromTile = toGetSilverFrom.Tile;
                eventData._toTile = SessionValues.chosenSettlement.Tile;
                eventData._eventFile = EventManagerHelper.availableEvents[DialogManager.selectedScrollButton];

                Packet packet = Packet.CreatePacketFromObject(nameof(EventManager), eventData);
                Network.listener.EnqueuePacket(packet);

                DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for event"));
            }
        }

        public static void TriggerEvent(IncidentDef eventToTrigger, Map targetMap)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(eventToTrigger.category, targetMap);
            parms.customLetterLabel = $"Event - {eventToTrigger.LabelCap}";
            parms.faction = FactionValues.neutralPlayer;
            parms.target = targetMap;

            eventToTrigger.Worker.TryExecute(parms);

            SaveManager.ForceSave();
        }

        public static void OnEventReceived(EventData eventData)
        {
            if (ClientValues.isReadyToPlay)
            {
                Map targetMap;
                if (eventData._toTile != -1) targetMap = Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == eventData._toTile).Map;
                else targetMap = Find.AnyPlayerHomeMap;

                IncidentDef eventToTrigger = DefDatabase<IncidentDef>.AllDefs.FirstOrDefault(fetch => fetch.defName == eventData._eventFile.DefName);
                if (eventToTrigger != null) TriggerEvent(eventToTrigger, targetMap);
            }
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

            //TODO
            //MAKE IT SO ALL MAPS ARE ACCOUNTED FOR
            Map toReturnTo = Find.AnyPlayerHomeMap;

            Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
            silverToReturn.stackCount = EventManagerHelper.availableEvents[DialogManager.selectedScrollButton].Cost;

            RimworldManager.PlaceThingIntoMap(silverToReturn, toReturnTo, ThingPlaceMode.Near, true);

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }
    }

    public static class EventManagerHelper
    {
        public static EventFile[] availableEvents;

        public static void SetValues(ServerGlobalData serverGlobalData) { availableEvents = serverGlobalData._eventValues; }
    }
}
