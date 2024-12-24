using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class NPCSettlementManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableNPCDestruction)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            NPCSettlementData data = Serializer.ConvertBytesToObject<NPCSettlementData>(packet.contents);

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

        public static void RemoveNPCSettlement(ServerClient client, PlanetNPCSettlement settlement)
        {
            if (!NPCSettlementManagerHelper.CheckIfSettlementFromTileExists(settlement.tile))
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried removing a non-existing NPC settlement");
            }
            
            else
            {
                DeleteSettlement(settlement);

                BroadcastSettlementDeletion(settlement);

                Logger.Warning($"[Delete NPC settlement] > {settlement.tile} > {client.userFile.Username}");
            }
        }

        private static void DeleteSettlement(PlanetNPCSettlement settlement)
        {
            List<PlanetNPCSettlement> finalSettlements = Master.worldValues.NPCSettlements.ToList();
            finalSettlements.Remove(NPCSettlementManagerHelper.GetSettlementFromTile(settlement.tile));
            Master.worldValues.NPCSettlements = finalSettlements.ToArray();
            Main_.SaveValueFile(ServerFileMode.World);
        }

        private static void BroadcastSettlementDeletion(PlanetNPCSettlement settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData = settlement;

            Packet packet = Packet.CreatePacketFromObject(nameof(NPCSettlementManager), data);
            NetworkHelper.SendPacketToAllClients(packet);
        }
    }

    public static class NPCSettlementManagerHelper
    {
        public static bool CheckIfSettlementFromTileExists(int tile)
        {
            foreach (PlanetNPCSettlement settlement in Master.worldValues.NPCSettlements.ToArray())
            {
                if (settlement.tile == tile) return true;
            }

            return false;
        }

        public static PlanetNPCSettlement GetSettlementFromTile(int tile)
        {
            return Master.worldValues.NPCSettlements.FirstOrDefault(fetch => fetch.tile == tile); ;
        }
    }
}
