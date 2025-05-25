using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Dialogs;
using GameClient.Misc;
using GameClient.TCP;
using GameClient.Values;
using RimWorld;
using Shared;
using Verse;
using static Shared.CommonEnumerators;

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

        public static void ShowEventMenu()
        {
            List<string> eventNames = new List<string>();

            foreach (EventFile eventFile in EventManagerHelper.availableEvents) eventNames.Add(eventFile.Name);

            Action a1 = delegate
            {
                RT_Dialog_YesNo d2 = new RT_Dialog_YesNo($"This event will cost you {EventManagerHelper.availableEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost} " +
                    $"silver, continue?", SendEvent, null);

                RT_Dialog_Base.PushNewDialog(d2);
            };

            RT_Dialog_ScrollButtons d1 = new RT_Dialog_ScrollButtons("Event Selector", "Choose the even you want to send",
                eventNames.ToArray(), a1.Invoke, null);

            RT_Dialog_Base.PushNewDialog(d1);
        }

        public static void SendEvent()
        {
            RT_Dialog_ScrollButtons.Instance.Close();

            //TODO
            //MAKE IT SO ALL MAPS ARE ACCOUNTED FOR
            Map toGetSilverFrom = Find.AnyPlayerHomeMap;

            if (!RimworldManager.CheckIfHasEnoughSilverInMap(toGetSilverFrom, EventManagerHelper.availableEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost))
            {
                RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "You do not have enough silver for this action!" }));
            }

            else
            {
                RimworldManager.RemoveThingFromSettlement(toGetSilverFrom, ThingDefOf.Silver, EventManagerHelper.availableEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost);

                EventData eventData = new EventData();
                eventData._stepMode = EventStepMode.Send;
                eventData._fromTile = toGetSilverFrom.Tile;
                eventData._toTile = SessionValues.ChosenSettlement.Tile;
                eventData._eventFile = EventManagerHelper.availableEvents[RT_Dialog_ScrollButtons.SelectedScrollButton];

                Network.Listener.EnqueuePacket(PacketHeader.EventManager, eventData);

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
            silverToReturn.stackCount = EventManagerHelper.availableEvents[RT_Dialog_ScrollButtons.SelectedScrollButton].Cost;

            RimworldManager.PlaceThingIntoMap(silverToReturn, toReturnTo, ThingPlaceMode.Near, true);

            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message("ERROR", new string[] { "Player is not currently available!" }));
        }
    }

    public static class EventManagerHelper
    {
        public static EventFile[] availableEvents;

        public static void SetValues(ServerGlobalData serverGlobalData) { availableEvents = serverGlobalData._eventValues; }
    }
}
