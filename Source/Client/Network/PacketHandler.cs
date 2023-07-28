using RimworldTogether.GameClient.Dialogs;
using RimworldTogether.GameClient.Managers;
using RimworldTogether.GameClient.Managers.Actions;
using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Patches;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.GameClient.Values;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using Verse;

namespace RimworldTogether.GameClient.Network
{
    public static class PacketHandlers
    {
        public static void HandlePacket(Packet packet)
        {
            Log.Message($"Packet > {packet.header}");

            switch (packet.header)
            {
                case "LoginResponsePacket":
                    LoginManager.ReceiveLoginResponse(packet);
                    break;

                case "ChatPacket":
                    ChatManager.ReceiveMessages(packet);
                    break;

                case "CommandPacket":
                    CommandManager.ParseCommand(packet);
                    break;

                case "TransferPacket":
                    TransferManager.ParseTransferPacket(packet);
                    break;

                case "FactionPacket":
                    FactionManager.ParseFactionPacket(packet);
                    break;

                case "VisitPacket":
                    VisitManager.ParseVisitPacket(packet);
                    break;

                case "OfflineVisitPacket":
                    OfflineVisitManager.ParseOfflineVisitPacket(packet);
                    break;

                case "RaidPacket":
                    RaidManager.ParseRaidPacket(packet);
                    break;

                case "SettlementPacket":
                    SettlementManager.ParseSettlementPacket(packet);
                    break;

                case "SpyPacket":
                    SpyManager.ParseSpyPacket(packet);
                    break;

                case "SitePacket":
                    SiteManager.ParseSitePacket(packet);
                    break;

                case "WorldPacket":
                    WorldManager.ParseWorldPacket(packet);
                    break;

                case "BreakPacket":
                    DialogManager.PopWaitDialog();
                    break;

                case "LoadFilePacket":
                    SavePatch.ReceiveSaveFromServer(packet);
                    break;

                case "PlayerRecountPacket":
                    ServerValues.SetServerPlayers(packet);
                    break;

                case "LikelihoodPacket":
                    DialogManager.PopWaitDialog();
                    LikelihoodManager.ChangeStructureLikelihood(packet);
                    break;

                case "EventPacket":
                    EventManager.ParseEventPacket(packet);
                    break;

                case "IllegalActionPacket":
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Kicked for ilegal actions!"));
                    break;

                case "UserUnavailablePacket":
                    DialogManager.PopWaitDialog();
                    DialogManager.PushNewDialog(new RT_Dialog_Error("Player is not currently available!"));
                    break;

                case "ServerValuesPacket":
                    ServerOverallJSON serverOverallJSON = Serializer.SerializeFromString<ServerOverallJSON>(packet.contents[0]);
                    ServerValues.SetServerParameters(serverOverallJSON);
                    ServerValues.SetAccountDetails(serverOverallJSON);
                    PlanetBuilder_Temp.SetWorldFeatures(serverOverallJSON);
                    EventManager.SetEventPrices(serverOverallJSON);
                    SiteManager.SetSiteDetails(serverOverallJSON);
                    SpyManager.SetSpyCost(serverOverallJSON);
                    DifficultyValues.SetDifficultyValues(serverOverallJSON);
                    break;
            }
        }
    }
}
