using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class NPCManager
    {
        [HandlesPacket(PacketHeader.NPCManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableNPCDestruction)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            NPCSettlementData data = Serializer.ConvertBytesToObject<NPCSettlementData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

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

        public static void RemoveNPCSettlement(ServerClient client, PlanetNPCSettlementDetails settlement)
        {
            if (!NPCSettlementManagerHelper.CheckIfSettlementFromTileExists(settlement.Tile))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried removing a non-existing NPC settlement");
            }

            else
            {
                DeleteSettlement(settlement);

                BroadcastSettlementDeletion(settlement);

                Printer.Warning($"[Delete NPC settlement] > {settlement.Tile} > {client.UserFile.Uid}");
            }
        }

        private static void DeleteSettlement(PlanetNPCSettlementDetails settlement)
        {
            List<PlanetNPCSettlementDetails> finalSettlements = Master.WorldValues.NPCSettlements.ToList();
            finalSettlements.Remove(NPCSettlementManagerHelper.GetSettlementFromTile(settlement.Tile));
            Master.WorldValues.NPCSettlements = finalSettlements.ToArray();
            Main_.SaveValueFile(ServerFileMode.World);
        }

        private static void BroadcastSettlementDeletion(PlanetNPCSettlementDetails settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData = settlement;

            NetworkHelper.SendPacketToAllClients(PacketHeader.NPCManager, data);
        }
    }

    public static class NPCSettlementManagerHelper
    {
        public static bool CheckIfSettlementFromTileExists(int tile)
        {
            foreach (PlanetNPCSettlementDetails settlement in Master.WorldValues.NPCSettlements.ToArray())
            {
                if (settlement.Tile == tile) return true;
            }

            return false;
        }

        public static PlanetNPCSettlementDetails GetSettlementFromTile(int tile)
        {
            return Master.WorldValues.NPCSettlements.FirstOrDefault(fetch => fetch.Tile == tile); ;
        }
    }
}
