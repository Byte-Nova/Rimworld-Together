using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using System;
using System.Reflection;
using Verse;

namespace RimworldTogether.GameClient.Network
{
    public static class PacketHandler
    {
        public static void HandlePacket(Packet packet)
        {
            Log.Message($"[Header] > {packet.header}");

            Type toUse = typeof(PacketHandler);
            MethodInfo methodInfo = toUse.GetMethod(packet.header);
            methodInfo.Invoke(packet.header, new object[] { packet });
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
            VisitManager.ParseVisitPacket(packet);
        }

        public static void OfflineVisitPacket(Packet packet)
        {
            OfflineVisitManager.ParseOfflineVisitPacket(packet);
        }

        public static void RaidPacket(Packet packet)
        {
            RaidManager.ParseRaidPacket(packet);
        }

        public static void SettlementPacket(Packet packet)
        {
            SettlementManager.ParseSettlementPacket(packet);
        }

        public static void SpyPacket(Packet packet)
        {
            SpyManager.ParseSpyPacket(packet);
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

        public static void LoadFilePartPacket(Packet packet)
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
            ServerOverallJSON serverOverallJSON = (ServerOverallJSON)ObjectConverter.ConvertBytesToObject(packet.contents);
            ServerValues.SetServerParameters(serverOverallJSON);
            ServerValues.SetAccountDetails(serverOverallJSON);
            PlanetBuilderHelper.SetWorldFeatures(serverOverallJSON);
            EventManager.SetEventPrices(serverOverallJSON);
            SiteManager.SetSiteDetails(serverOverallJSON);
            SpyManager.SetSpyCost(serverOverallJSON);
            CustomDifficultyManager.SetCustomDifficulty(serverOverallJSON);
        }
    }
}
