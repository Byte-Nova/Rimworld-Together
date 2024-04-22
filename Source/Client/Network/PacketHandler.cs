using Shared;
using System;
using System.Reflection;
using Verse;

namespace GameClient
{
    //Class that handles the management of all the received packets

    public static class PacketHandler
    {
        //Function that opens handles the action that the packet should do, then sends it to the correct one below

        public static void HandlePacket(Packet packet)
        {
            if (ClientValues.verboseBool) Log.Message($"[Header] > {packet.header}");

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
            ChatManager.ReceiveMessages(packet);
        }

        public static void CommandPacket(Packet packet)
        {
            CommandManager.ParseCommand(packet);
        }

        public static void TransferPacket(Packet packet)
        {
            TransferManager.ParseTransferPacket(packet);
        }

        public static void FactionPacket(Packet packet)
        {
            OnlineFactionManager.ParseFactionPacket(packet);
        }

        public static void VisitPacket(Packet packet)
        {
            OnlineVisitManager.ParseVisitPacket(packet);
        }

        public static void OfflineVisitPacket(Packet packet)
        {
            OfflineVisitManager.ParseOfflineVisitPacket(packet);
        }

        public static void RaidPacket(Packet packet)
        {
            OfflineRaidManager.ParseRaidPacket(packet);
        }

        public static void SettlementPacket(Packet packet)
        {
            PlanetManager.ParseSettlementPacket(packet);
        }

        public static void SpyPacket(Packet packet)
        {
            OfflineSpyManager.ParseSpyPacket(packet);
        }

        public static void SitePacket(Packet packet)
        {
            SiteManager.ParseSitePacket(packet);
        }

        public static void WorldPacket(Packet packet)
        {
            WorldManager.ParseWorldPacket(packet);
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

        public static void LikelihoodPacket(Packet packet)
        {
            DialogManager.PopWaitDialog();
            LikelihoodManager.ChangeStructureLikelihood(packet);
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
            ServerOverallJSON serverOverallJSON = (ServerOverallJSON)Serializer.ConvertBytesToObject(packet.contents);
            ServerValues.SetServerParameters(serverOverallJSON);
            ServerValues.SetAccountDetails(serverOverallJSON);
            PlanetManagerHelper.SetWorldFeatures(serverOverallJSON);
            EventManager.SetEventPrices(serverOverallJSON);
            SiteManager.SetSiteDetails(serverOverallJSON);
            OfflineSpyManager.SetSpyCost(serverOverallJSON);
            CustomDifficultyManager.SetCustomDifficulty(serverOverallJSON);
        }

        //Empty functions

        public static void KeepAlivePacket()
        {
            //EMPTY
        }

        public static void ResetSavePacket()
        {
            //Empty
        }

        public static void MapPacket()
        {
            //Empty
        }

        public static void RegisterClientPacket()
        {
            //Empty
        }

        public static void LoginClientPacket()
        {
            //Empty
        }

        public static void CustomDifficultyPacket()
        {
            //Empty
        }
    }
}
