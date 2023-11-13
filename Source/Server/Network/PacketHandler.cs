using System.Reflection;
using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Users;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Network
{
    public static class PacketHandler
    {
        public static void HandlePacket(ServerClient client, Packet packet)
        {
            //Logger.WriteToConsole($"[Header] > {packet.header}");

            Type toUse = typeof(PacketHandler);
            MethodInfo methodInfo = toUse.GetMethod(packet.header);
            methodInfo.Invoke(packet.header, new object[] { client, packet });
        }

        public static void LoginClientPacket(ServerClient client, Packet packet)
        {
            UserLogin.TryLoginUser(client, packet);
        }

        public static void RegisterClientPacket(ServerClient client, Packet packet)
        {
            UserRegister.TryRegisterUser(client, packet);
        }

        public static void RequestSavePartPacket(ServerClient client, Packet packet)
        {
            SaveManager.LoadUserGamePart(client);
        }

        public static void ReceiveFilePartPacket(ServerClient client, Packet packet)
        {
            SaveManager.SaveUserGamePart(client, packet);
        }

        public static void LikelihoodPacket(ServerClient client, Packet packet)
        {
            LikelihoodManager.ChangeUserLikelihoods(client, packet);
        }

        public static void TransferPacket(ServerClient client, Packet packet)
        {
            TransferManager.ParseTransferPacket(client, packet);
        }

        public static void SitePacket(ServerClient client, Packet packet)
        {
            SiteManager.ParseSitePacket(client, packet);
        }

        public static void VisitPacket(ServerClient client, Packet packet)
        {
            VisitManager.ParseVisitPacket(client, packet);
        }

        public static void OfflineVisitPacket(ServerClient client, Packet packet)
        {
            OfflineVisitManager.ParseOfflineVisitPacket(client, packet);
        }

        public static void ChatPacket(ServerClient client, Packet packet)
        {
            ChatManager.ParseClientMessages(client, packet);
        }

        public static void FactionPacket(ServerClient client, Packet packet)
        {
            FactionManager.ParseFactionPacket(client, packet);
        }

        public static void MapPacket(ServerClient client, Packet packet)
        {
            MapManager.SaveUserMap(client, packet);
        }

        public static void RaidPacket(ServerClient client, Packet packet)
        {
            RaidManager.ParseRaidPacket(client, packet);
        }

        public static void SpyPacket(ServerClient client, Packet packet)
        {
            SpyManager.ParseSpyPacket(client, packet);
        }

        public static void SettlementPacket(ServerClient client, Packet packet)
        {
            SettlementManager.ParseSettlementPacket(client, packet);
        }

        public static void EventPacket(ServerClient client, Packet packet)
        {
            EventManager.ParseEventPacket(client, packet);
        }

        public static void WorldPacket(ServerClient client, Packet packet)
        {
            WorldManager.ParseWorldPacket(client, packet);
        }

        public static void CustomDifficultyPacket(ServerClient client, Packet packet)
        {
            CustomDifficultyManager.ParseDifficultyPacket(client, packet);
        }

        public static void ResetSavePacket(ServerClient client, Packet packet)
        {
            SaveManager.ResetClientSave(client);
        }
    }
}
