using Shared;
using static Shared.CommonEnumerators;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer
{
    public static class NPCSettlementManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
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
            if (!Master.serverConfig.AllowNPCModifications) return;
            else
            {
                if (!NPCSettlementManagerHelper.CheckIfSettlementFromTileExists(settlement.tile))
                {
                    ResponseShortcutManager.SendIllegalPacket(client, "Tried removing a non-existing NPC settlement");
                }
                else
                {
                    DeleteSettlement(settlement);

                    BroadcastSettlementDeletion(settlement, client);

                    Logger.Warning($"[Delete NPC settlement] > {settlement.tile} > {client.userFile.Username}");
                }
            }
        }

        public static void AddNPCSettlement(ServerClient client, PlanetNPCSettlement settlement) 
        {
            if (!Master.serverConfig.AllowNPCModifications) return;
            if (NPCSettlementManagerHelper.CheckIfSettlementFromTileExists(settlement.tile))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Tried adding a settlement on an existing settlement");
            }
            else
            {
                AddSettlement(settlement);

                BroadcastSettlementAddition(settlement, client);

                Logger.Warning($"[Add NPC settlement] > {settlement.tile} > {client.userFile.Username}");
            }
        }

        private static void DeleteSettlement(PlanetNPCSettlement settlement)
        {
            List<PlanetNPCSettlement> finalSettlements = Master.worldValues.NPCSettlements.ToList();
            finalSettlements.Remove(NPCSettlementManagerHelper.GetSettlementFromTile(settlement.tile));
            Master.worldValues.NPCSettlements = finalSettlements.ToArray();
            Main_.SaveValueFile(ServerFileMode.World);
        }

        private static void AddSettlement(PlanetNPCSettlement settlement) 
        {
            List<PlanetNPCSettlement> finalSettlements = Master.worldValues.NPCSettlements.ToList();
            finalSettlements.Add(settlement);
            Master.worldValues.NPCSettlements = finalSettlements.ToArray();
        }
        private static void BroadcastSettlementDeletion(PlanetNPCSettlement settlement, ServerClient client)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData = settlement;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.NPCSettlementPacket), data);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }

        private static void BroadcastSettlementAddition(PlanetNPCSettlement settlement, ServerClient client) 
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Add;
            data._settlementData = settlement;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.NPCSettlementPacket), data);
            NetworkHelper.SendPacketToAllClients(packet, client);
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
    public static class NPCFactionManager 
    {
        public static void AddNPCFaction(PlanetNPCFaction faction, ServerClient client)
        {
            try
            {
                List<PlanetNPCFaction> currentFactions = Master.worldValues.NPCFactions.ToList();
                currentFactions.Add(faction);
                Master.worldValues.NPCFactions = currentFactions.ToArray();
                BroadCastNewNPCFaction(faction, client);
                Logger.Warning($"[Add NPC settlement] > {faction.defName} > {client.userFile.Username}");
            }
            catch (Exception ex){ Logger.Warning($"Failed to generate faction, Reason: {ex.ToString()}"); }
        }

        public static void BroadCastNewNPCFaction(PlanetNPCFaction faction, ServerClient client) 
        {
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.NPCFactionPacket), faction);
            NetworkHelper.SendPacketToAllClients(packet, client);
        }
    }
}
