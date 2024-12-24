using RimWorld;
using Shared;
using System;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class AidManager
    {
        public static void ParsePacket(Packet packet)
        {
            AidData data = Serializer.ConvertBytesToObject<AidData>(packet.contents);

            switch (data._stepMode)
            {
                case AidStepMode.Send:
                    //Empty
                    break;

                case AidStepMode.Receive:
                    ReceiveAidRequest(data);
                    break;

                case AidStepMode.Accept:
                    OnAidAccept();
                    break;

                case AidStepMode.Reject:
                    OnAidReject(data);
                    break;
            }
        }

        private static void ReceiveAidRequest(AidData data)
        {
            Action toDoYes = delegate { AcceptAid(data); };
            Action toDoNo = delegate { RejectAid(data); };

            DialogManager.PushNewDialog(new RT_Dialog_YesNo("You are receiving aid, accept?", toDoYes, toDoNo));
        }

        public static void SendAidRequest()
        {
            AidData aidData = new AidData();
            aidData._stepMode = AidStepMode.Send;
            aidData._fromTile = Find.AnyPlayerHomeMap.Tile;
            aidData._toTile = SessionValues.chosenSettlement.Tile;

            Pawn toGet = RimworldManager.GetAllSettlementsPawns(Faction.OfPlayer, false)[DialogManager.dialogButtonListingResultInt];
            aidData._humanData = HumanScriber.HumanToString(toGet);
            RimworldManager.RemovePawnFromGame(toGet);

            Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), aidData);
            Network.listener.EnqueuePacket(packet);

            DialogManager.PushNewDialog(new RT_Dialog_Wait("Waiting for server response"));
        }

        private static void OnAidAccept()
        {
            DialogManager.PopWaitDialog();

            RimworldManager.GenerateLetter("Sent aid",
                "You have sent aid towards a settlement! The owner will receive the news soon",
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void OnAidReject(AidData data)
        {
            DialogManager.PopWaitDialog();

            Map map = Find.World.worldObjects.SettlementAt(data._fromTile).Map;
            Pawn pawn = HumanScriber.StringtoHuman(data._humanData);
            RimworldManager.PlaceThingIntoMap(pawn, map, ThingPlaceMode.Near, true);

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }

        private static void AcceptAid(AidData data)
        {
            Map map = Find.World.worldObjects.SettlementAt(data._toTile).Map;
            Pawn pawn = HumanScriber.StringtoHuman(data._humanData);
            RimworldManager.PlaceThingIntoMap(pawn, map, ThingPlaceMode.Near, true, true);

            data._stepMode = AidStepMode.Accept;
            Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
            Network.listener.EnqueuePacket(packet);

            RimworldManager.GenerateLetter("Received aid",
                "You have received aid from a player! The pawn should come to help soon",
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void RejectAid(AidData data)
        {
            data._stepMode = AidStepMode.Reject;
            Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
            Network.listener.EnqueuePacket(packet);
        }
    }
}
