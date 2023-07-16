using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class PacketHandler
    {
        public static void HandlePacket(Client client, Packet packet)
        {
            #if DEBUG
            Debug.WriteLine(packet.header);
            #endif

            switch (packet.header)
            {
                case "LoginClientPacket":
                    UserLogin.TryLoginUser(client, packet);
                    break;

                case "RegisterClientPacket":
                    UserRegister.TryRegisterUser(client, packet);
                    break;

                case "SaveFilePacket":
                    SaveManager.SaveUserGame(client, packet);
                    break;

                case "LikelihoodPacket":
                    LikelihoodManager.ChangeUserLikelihoods(client, packet);
                    break;

                case "TransferPacket":
                    TransferManager.ParseTransferPacket(client, packet);
                    break;

                case "SitePacket":
                    SiteManager.ParseSitePacket(client, packet);
                    break;

                case "VisitPacket":
                    VisitManager.ParseVisitPacket(client, packet);
                    break;

                case "OfflineVisitPacket":
                    OfflineVisitManager.ParseOfflineVisitPacket(client, packet);
                    break;

                case "ChatPacket":
                    ChatManager.ParseClientMessages(client, packet);
                    break;

                case "FactionPacket":
                    FactionManager.ParseFactionPacket(client, packet);
                    break;

                case "MapPacket":
                    SaveManager.SaveUserMap(client, packet);
                    break;

                case "RaidPacket":
                    RaidManager.ParseRaidPacket(client, packet);
                    break;

                case "SpyPacket":
                    SpyManager.ParseSpyPacket(client, packet);
                    break;

                case "SettlementPacket":
                    SettlementManager.ParseSettlementPacket(client, packet);
                    break;

                case "EventPacket":
                    EventManager.ParseEventPacket(client, packet);
                    break;

                case "WorldPacket":
                    WorldManager.ParseWorldPacket(client, packet);
                    break;

                case "ResetSavePacket":
                    SaveManager.ResetClientSave(client);
                    break;
            }
        }
    }
}
