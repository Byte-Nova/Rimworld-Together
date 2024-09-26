using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class WorldObjectManager
    {
        public static void ParseNewWorldObjectsPacket(ServerClient client, Packet packet)
        {
            NewWorldObjects newWorldObjects = Serializer.ConvertBytesToObject<NewWorldObjects>(packet.contents);
            if (Master.serverConfig.AllowNPCModifications)
            {
                if ((!client.userFile.IsAdmin && newWorldObjects._npcSettlements.Count() > 0 && !Master.serverConfig.AllowNPCModifications) || !Master.serverConfig.AllowNPCModificationsForNonAdmin)
                {
                    Logger.Warning($"User {client.userFile.Username} tried changing settlements. but they are not an admin");
                }
                else
                {
                    foreach (PlanetNPCFaction faction in newWorldObjects._planetNPCFaction)
                    {
                        NPCFactionManager.AddNPCFaction(faction, client);
                    }
                    foreach (NPCSettlementData settlement in newWorldObjects._npcSettlements)
                    {
                        switch (settlement._stepMode)
                        {
                            case SettlementStepMode.Add:
                                NPCSettlementManager.AddNPCSettlement(client, settlement._settlementData);
                                break;
                            case SettlementStepMode.Remove:
                                NPCSettlementManager.RemoveNPCSettlement(client, settlement._settlementData);
                                break;
                        }
                    }
                }
            } 
            else 
            {
                Logger.Message($"User {client.userFile.Username} tried modifying NPC data, consider turning on npc modifications in the settings.", LogImportanceMode.Verbose);
            }
            foreach (PlayerSettlementData settlement in newWorldObjects._playerSettlements)
            {
                switch (settlement._stepMode)
                {
                    case SettlementStepMode.Add:
                        SettlementManager.AddSettlement(client, settlement);
                        break;
                    case SettlementStepMode.Remove:
                        SettlementManager.RemoveSettlement(client, settlement);
                        break;
                }
            }
            Main_.SaveValueFile(ServerFileMode.World);
        }
    }
}
