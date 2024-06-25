using RimWorld;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GameClient
{
    public static class AidManager
    {
        public static void ParsePacket(Packet packet)
        {
            AidData data = (AidData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.stepMode)
            {
                case CommonEnumerators.AidStepMode.Send:
                    //Empty
                    break;

                case CommonEnumerators.AidStepMode.Receive:
                    ReceiveAidRequest(data);
                    break;

                case CommonEnumerators.AidStepMode.Accept:
                    OnAidAccept();
                    break;

                case CommonEnumerators.AidStepMode.Reject:
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
            aidData.stepMode = CommonEnumerators.AidStepMode.Send;
            aidData.fromTile = Find.AnyPlayerHomeMap.Tile;
            aidData.toTile = ClientValues.chosenSettlement.Tile;

            Pawn toGet = RimworldManager.GetAllSettlementPawns(Faction.OfPlayer, false)[DialogManager.dialgButtonListingResultInt];
            aidData.humanData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(toGet));
            RimworldManager.RemovePawnFromGame(toGet);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), aidData);
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

            Map map = Find.World.worldObjects.SettlementAt(data.fromTile).Map;

            HumanData humanData = (HumanData)Serializer.ConvertBytesToObject(data.humanData);
            Pawn pawn = HumanScribeManager.StringToHuman(humanData);
            RimworldManager.PlaceThingIntoMap(pawn, map, ThingPlaceMode.Near, true);

            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }

        private static void AcceptAid(AidData data)
        {
            Map map = Find.World.worldObjects.SettlementAt(data.toTile).Map;

            HumanData humanData = (HumanData)Serializer.ConvertBytesToObject(data.humanData);
            Pawn pawn = HumanScribeManager.StringToHuman(humanData);
            RimworldManager.PlaceThingIntoMap(pawn, map, ThingPlaceMode.Near, true);

            data.stepMode = CommonEnumerators.AidStepMode.Accept;
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
            Network.listener.EnqueuePacket(packet);

            RimworldManager.GenerateLetter("Reveived aid",
                "You have received aid from a player! The pawn should come to help soon",
                LetterDefOf.PositiveEvent);

            SaveManager.ForceSave();
        }

        private static void RejectAid(AidData data)
        {
            data.stepMode = CommonEnumerators.AidStepMode.Reject;
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), data);
            Network.listener.EnqueuePacket(packet);
        }
    }
}
