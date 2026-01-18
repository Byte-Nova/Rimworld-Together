using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using Shared.Details.Planet;
using Shared.Misc;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{

    public static class NPCManager
    {
        [HandlesPacket(PacketHeader.NPCManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.EnableNPCDestruction)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            NPCSettlementData data = Serializer.ConvertBytesToObject<NPCSettlementData>(bytes);

            switch (data._stepMode)
            {
                case SettlementStepMode.Add:
                    ResponseShortcutManager.SendIllegalPacket(client, "Tried to execute unimplemented action");
                    break;

                case SettlementStepMode.Remove:
                    RemoveNPCSettlement(client, data._settlementData);
                    break;
            }
        }

        public static void RemoveNPCSettlement(ServerClient client, NPCSettlementDetail settlement)
        {
            if (!NPCSettlementManagerHelper.CheckIfSettlementFromTileExists(settlement.Tile))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried removing a non-existing NPC settlement");
            }

            else
            {
                DeleteSettlement(settlement);

                BroadcastSettlementDeletion(settlement);

                Printer.Warning($"[Delete NPC settlement] > {settlement.Tile} > {client.UserFile.Username}");
            }
        }

        private static void DeleteSettlement(NPCSettlementDetail settlement)
        {
            List<NPCSettlementDetail> finalSettlements = Master.WorldValues.NPCSettlements.ToList();
            finalSettlements.Remove(NPCSettlementManagerHelper.GetSettlementFromTile(settlement.Tile));
            Master.WorldValues.NPCSettlements = finalSettlements.ToArray();
            Master.WorldValues.Save();
        }

        private static void BroadcastSettlementDeletion(NPCSettlementDetail settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData = settlement;

            ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.NPCManager, data);
        }
    }

    public static class NPCSettlementManagerHelper
    {
        public static bool CheckIfSettlementFromTileExists(int tile)
        {
            foreach (NPCSettlementDetail settlement in Master.WorldValues.NPCSettlements.ToArray())
            {
                if (settlement.Tile == tile) return true;
            }

            return false;
        }

        public static NPCSettlementDetail GetSettlementFromTile(int tile)
        {
            return Master.WorldValues.NPCSettlements.FirstOrDefault(fetch => fetch.Tile == tile); ;
        }
    }
}
