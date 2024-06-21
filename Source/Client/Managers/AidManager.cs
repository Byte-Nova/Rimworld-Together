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
                    //Do nothing
                    break;

                case CommonEnumerators.AidStepMode.Receive:
                    ReceiveAidRequest(data);
                    break;

                case CommonEnumerators.AidStepMode.Recover:
                    RecoverAidRequest(data);
                    break;
            }
        }

        public static void SendAidRequest()
        {
            AidData aidData = new AidData();
            aidData.stepMode = CommonEnumerators.AidStepMode.Send;
            aidData.fromTile = Find.AnyPlayerHomeMap.Tile;
            aidData.toTile = ClientValues.chosenSettlement.Tile;

            Pawn toGet = RimworldManager.GetAllSettlementPawns(Faction.OfPlayer, false)[DialogManager.dialogButtonListingResult];
            aidData.humanData = Serializer.ConvertObjectToBytes(HumanScribeManager.HumanToString(toGet));

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.AidPacket), aidData);
            Network.listener.EnqueuePacket(packet);
            Logger.Warning("Sent");
        }

        private static void ReceiveAidRequest(AidData data)
        {
            Logger.Warning("Received");
        }

        private static void RecoverAidRequest(AidData data)
        {
            Logger.Warning("Recovered");
        }
    }
}
