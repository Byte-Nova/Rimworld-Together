using GameClient.Dialogs;
using TCPNetwork.Packets;
using Shared;
using GameClient.Misc;
using Shared.Files.Maps;

namespace GameClient.Managers
{
    public static class InformationManager
    {
        [HandlesPacket(PacketHeader.InformationManager)]
        private static void ParsePacket(byte[] bytes)
        {
            InformationData data = Serializer.ConvertBytesToObject<InformationData>(bytes);

            switch (data._stepMode)
            {
                case InformationData.InfoStepMode.Connection:
                    ReceiveInformation(data);
                    break;

                case InformationData.InfoStepMode.Wealth:
                    ReceiveWealth(data);
                    break;
            }
        }

        public static void AskForInformation()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for server"));

            InformationData data = new InformationData();
            data._stepMode = InformationData.InfoStepMode.Connection;
            data._settlementTile = SessionHandler.ChosenSettlement.Tile;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.InformationManager, data);
        }

        public static void AskForWealth()
        {
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Wait("Waiting for server"));

            InformationData data = new InformationData();
            data._stepMode = InformationData.InfoStepMode.Wealth;
            data._settlementTile = SessionHandler.ChosenSettlement.Tile;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.InformationManager, data);
        }

        public static void ReceiveInformation(InformationData data)
        {
            RT_Dialog_Wait.Instance.Close();

            string connectionString = data._isPlayerOnline ? "connected" : "not connected";

            string title = "Information";
            string[] messages = new string[] { $"The player is {connectionString}" };
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message(title, messages));
        }

        public static void ReceiveWealth(InformationData data)
        {
            RT_Dialog_Wait.Instance.Close();

            MapFile file = Serializer.ConvertBytesToObject<MapFile>(data._settlementRawData);

            string title = "Information";
            string[] messages = new string[] { $"The wealth of the map is {file.Wealth}" };
            RT_Dialog_Base.PushNewDialog(new RT_Dialog_Message(title, messages));
        }
    }
}
