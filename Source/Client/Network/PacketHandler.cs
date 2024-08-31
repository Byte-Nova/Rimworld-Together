using Shared;
using System;
using System.Linq;
using System.Reflection;

namespace GameClient
{
    //Class that handles the management of all the received packets

    public static class PacketHandler
    {
        //Packet headers in this array won't output into the logs by default

        private static readonly string[] ignoreLogPackets =
        {
            nameof(KeepAlivePacket),
            nameof(OnlineActivityPacket)
        };

        //Function that opens handles the action that the packet should do, then sends it to the correct one below

        public static void HandlePacket(Packet packet)
        {
            if (ClientValues.verboseBool && !ignoreLogPackets.Contains(packet.header)) Logger.Message($"[N] > {packet.header}");
            else if (ClientValues.extremeVerboseBool) Logger.Message($"[N] > {packet.header}");

            Action toDo = delegate
            {
                Type toUse = typeof(PacketHandler);
                MethodInfo methodInfo = toUse.GetMethod(packet.header);
                methodInfo.Invoke(packet.header, new object[] { packet });
            };

            if (packet.requiresMainThread) Master.threadDispatcher.Enqueue(toDo);
            else toDo();
        }

        public static void LoginResponsePacket(Packet packet)
        {
            LoginManager.ReceiveLoginResponse(packet);
        }

        public static void ChatPacket(Packet packet)
        {
            ChatManager.ParsePacket(packet);
        }

        public static void CommandPacket(Packet packet)
        {
            CommandManager.ParseCommand(packet);
        }

        public static void TransferPacket(Packet packet)
        {
            TransferManager.ParseTransferPacket(packet);
        }

        public static void MarketPacket(Packet packet)
        {
            MarketManager.ParseMarketPacket(packet);
        }

        public static void AidPacket(Packet packet)
        {
            AidManager.ParsePacket(packet);
        }

        public static void FactionPacket(Packet packet)
        {
            FactionManager.ParseFactionPacket(packet);
        }

        public static void OnlineActivityPacket(Packet packet)
        {
            OnlineActivityManager.ParseOnlineActivityPacket(packet);
        }

        public static void OfflineActivityPacket(Packet packet)
        {
            OfflineActivityManager.ParseOfflineActivityPacket(packet);
        }

        public static void SettlementPacket(Packet packet)
        {
            PlayerSettlementManager.ParsePacket(packet);
        }

        public static void NPCSettlementPacket(Packet packet)
        {
            NPCSettlementManager.ParsePacket(packet);
        }

        public static void SitePacket(Packet packet)
        {
            SiteManager.ParseSitePacket(packet);
        }

        public static void RoadPacket(Packet packet)
        {
            RoadManager.ParsePacket(packet);
        }

        public static void CaravanPacket(Packet packet)
        {
            CaravanManager.ParsePacket(packet);
        }

        public static void WorldPacket(Packet packet)
        {
            PlanetGeneratorManager.ParseWorldPacket(packet);
        }

        public static void BreakPacket(Packet packet)
        {
            DialogManager.PopWaitDialog();
        }

        public static void RequestSavePartPacket(Packet packet)
        {
            SaveManager.SendSavePartToServer();
        }

        public static void ReceiveSavePartPacket(Packet packet)
        {
            SaveManager.ReceiveSavePartFromServer(packet);
        }

        public static void PlayerRecountPacket(Packet packet)
        {
            ServerValues.SetServerPlayers(packet);
        }

        public static void GoodwillPacket(Packet packet)
        {
            DialogManager.PopWaitDialog();
            GoodwillManager.ChangeStructureGoodwill(packet);
        }

        public static void EventPacket(Packet packet)
        {
            EventManager.ParseEventPacket(packet);
        }

        public static void IllegalActionPacket(Packet packet)
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Kicked for ilegal actions!"));
        }

        public static void UserUnavailablePacket(Packet packet)
        {
            DialogManager.PopWaitDialog();
            DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
        }

        public static void ServerValuesPacket(Packet packet)
        {
            ServerGlobalData serverGlobalData = Serializer.ConvertBytesToObject<ServerGlobalData>(packet.contents);
            ServerValues.SetValues(serverGlobalData);
            SessionValues.SetValues(serverGlobalData);
            EventManagerHelper.SetValues(serverGlobalData);
            SiteManager.SetValues(serverGlobalData);
            DifficultyManager.SetValues(serverGlobalData);
            PlayerSettlementManagerHelper.SetValues(serverGlobalData);
            NPCSettlementManagerHelper.SetValues(serverGlobalData);
            PlayerSiteManagerHelper.SetValues(serverGlobalData);
            CaravanManagerHelper.SetValues(serverGlobalData);
            RoadManagerHelper.SetValues(serverGlobalData);
            PollutionManagerHelper.SetValues(serverGlobalData);
        }

        //Empty functions

        public static void KeepAlivePacket(Packet packet) { }

        public static void ResetSavePacket(Packet packet) { }

        public static void MapPacket(Packet packet) { }

        public static void RegisterClientPacket(Packet packet) { }

        public static void LoginClientPacket(Packet packet) { }

        public static void CustomDifficultyPacket(Packet packet) { }
    }
}
