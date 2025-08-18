using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.Values;
using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;

namespace GameClient.Managers
{
    public static class EventManager
    {
        [HandlesPacket(PacketHeader.EventManager)]
        private static void ParsePacket(byte[] bytes)
        {
            EventData data = Serializer.ConvertBytesToObject<EventData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case EventStepMode.Send:
                    OnEventSent();
                    break;

                case EventStepMode.Receive:
                    OnEventReceived(data);
                    break;

                case EventStepMode.Recover:
                    OnRecoverEventSilver();
                    break;
            }
        }

        public static void SendExistingEventsToServer()
        {          
            List<EventFile> existingEvents = new List<EventFile>();
            foreach (IncidentDef incident in DefDatabase<IncidentDef>.AllDefs)
            {
                EventFile file = new EventFile();
                file.Name = incident.LabelCap;
                file.DefName = incident.defName;
                file.Cost = 500;
                file.IsEnabled = true;

                existingEvents.Add(file);
            }

            EventData eventData = new EventData();
            eventData._stepMode = EventStepMode.Set;
            eventData._eventFiles = existingEvents.ToArray();

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.EventManager, eventData);
        }

        public static void ShowEventMenu()
        {
            List<string> eventNames = new List<string>();
            foreach (EventFile eventFile in EventManagerH.EnabledEvents) eventNames.Add(eventFile.Name);

            Action a1 = delegate
            {
                RT_Dialog_YesNo d2 = new RT_Dialog_YesNo($"This event will cost you {EventManagerH.EnabledEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost} " +
                    $"silver, continue?", SendEvent, null);

                RT_Dialog_Base.PushNewDialog(d2);
            };

            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Event Selector", "Choose the event you want to send",
                eventNames.ToArray(), a1.Invoke, null);

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void ShowEventTweakerMenu()
        {
            string title = "Event Manager";
            string description = "Configure the availability of each event";
            string[] values = { "Disabled", "Enabled" };

            List<string> eventNames = new List<string>();
            foreach (EventFile ev in EventManagerH.AvailableEvents) eventNames.Add(ev.Name);

            List<int> defaultValues = new List<int>();
            foreach (EventFile ev in EventManagerH.AvailableEvents) defaultValues.Add(ev.IsEnabled == true ? 1 : 0);

            Action toDo = delegate
            {
                for (int i = 0; i < EventManagerH.AvailableEvents.Length; i++)
                {
                    EventFile file = EventManagerH.AvailableEvents[i];
                    file.IsEnabled = RT_Dialog_ListingWithTuple.DialogTupleListingResultInt[i] == 1 ? true : false;
                }

                EventData data = new EventData();
                data._stepMode = CommonEnumerators.EventStepMode.Customize;
                data._eventFiles = EventManagerH.AvailableEvents;
                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.EventManager, data);

                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("SUCCESS",
                    new string[] { "Changes will apply to new connecting players" }));
            };

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_ListingWithTuple(title, description, eventNames.ToArray(), values, defaultValues.ToArray(), toDo));
        }

        public static void SendEvent()
        {
            RT_Dialog_ScrollButtons.Instance.Close();

            //TODO
            //MAKE IT SO ALL MAPS ARE ACCOUNTED FOR
            Map toGetSilverFrom = Find.AnyPlayerHomeMap;

            if (!RimworldManager.CheckIfHasEnoughSilverInMap(toGetSilverFrom, EventManagerH.EnabledEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost))
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You do not have enough silver for this action!" }));
            }

            else
            {
                RimworldManager.RemoveThingFromSettlement(toGetSilverFrom, ThingDefOf.Silver, EventManagerH.EnabledEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost);

                EventData eventData = new EventData();
                eventData._stepMode = EventStepMode.Send;
                eventData._fromTile = toGetSilverFrom.Tile;
                eventData._toTile = SessionValues.ChosenSettlement.Tile;
                eventData._eventFile = EventManagerH.EnabledEvents[RT_Dialog_ScrollButtons.SelectedScrollButton];

                ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.EventManager, eventData);

                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for event"));
            }
        }

        public static void TriggerEvent(IncidentDef eventToTrigger, Map targetMap)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(eventToTrigger.category, targetMap);
            parms.customLetterLabel = $"Event - {eventToTrigger.LabelCap}";
            parms.faction = ClientValues.NeutralPlayer;
            parms.target = targetMap;

            eventToTrigger.Worker.TryExecute(parms);

            SaveManager.ForceSave();
        }

        public static void OnEventReceived(EventData eventData)
        {
            if (ClientValues.IsReadyToPlay)
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
            RT_Dialog_Wait.Instance.Close();

            RimworldManager.GenerateLetter("Event sent!", "Your event has been sent!",
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void OnRecoverEventSilver()
        {
            RT_Dialog_Wait.Instance.Close();

            //TODO
            //MAKE IT SO ALL MAPS ARE ACCOUNTED FOR
            Map toReturnTo = Find.AnyPlayerHomeMap;

            Thing silverToReturn = ThingMaker.MakeThing(ThingDefOf.Silver);
            silverToReturn.stackCount = EventManagerH.EnabledEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost;

            RimworldManager.PlaceThingIntoMap(silverToReturn, toReturnTo, ThingPlaceMode.Near, true);

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player is not currently available!" }));
        }
    }

    public static class EventManagerH
    {
        public static EventFile[] AvailableEvents { get; private set; } = null;

        public static EventFile[] EnabledEvents { get; private set; } = null;

        public static void SetValues(ServerGlobalData serverGlobalData) 
        { 
            AvailableEvents = serverGlobalData._eventValues;
            EnabledEvents = AvailableEvents.Where(fetch => fetch.IsEnabled).ToArray();
        }
    }
}
