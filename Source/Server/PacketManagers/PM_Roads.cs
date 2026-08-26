using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Details.Planet;
using RTShared.Files;
using RTShared.Files.Configs;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Road;

namespace RTServer.PacketManagers
{
    public class PM_Roads : PM_Base
    {
        [HandlesPacket(PacketHeader.Road)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanRoad(client.GetData<FL_Player>(), Master.ActionConfigs.RoadAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_Road data = Serializer.ConvertBytesToObject<PKT_Road>(bytes);

                switch (data.CurrentStepMode)
                {
                    case StepMode.Add:
                        AddRoad(client, data);
                        break;

                    case StepMode.Remove:
                        RemoveRoad(client, data);
                        break;
                    
                    case StepMode.Bulk:
                        AddRoadsBulk(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetRoadTimer(client.GetData<FL_Player>());
            }
        }

        private static void AddRoad(ServerClient client, PKT_Road packet)
        {
            RoadDetail toAdd = FindRoadFile(packet.Roads[0].Tile);
            
            if (toAdd != null) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                Master.RoadFile.Add(packet.Roads[0]);
                ServerNetwork.SendPacketToAllClients(PacketHeader.Road, packet);
                InformationDisplayer.DisplayAddRoad(packet.Roads[0].Tile.ToString());
            }
        }

        private static void RemoveRoad(ServerClient client, PKT_Road packet)
        {
            RoadDetail toRemove = FindRoadFile(packet.Roads[0].Tile);
            
            if (toRemove == null) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                Master.RoadFile.Remove(toRemove);
                ServerNetwork.SendPacketToAllClients(PacketHeader.Road, packet);
                InformationDisplayer.DisplayRemoveRoad(packet.Roads[0].Tile.ToString());
            }
        }

        private static void AddRoadsBulk(ServerClient client, PKT_Road packet)
        {
            if (!client.GetData<FL_Player>().IsAdmin && PM_World.CheckIfWorldExists()) client.Listener.MarkForDisconnect();
            else
            {
                Master.RoadFile.AddBulk(packet.Roads);
                client.Listener.EnqueuePacket(PacketHeader.Road, packet);
                Printer.Warning($"[Set roads] > {client.GetData<FL_Player>().Username}");   
            }
        }

        public static List<RoadDetail> GetAllRoads() { return Master.RoadFile.Roads; }

        private static RoadDetail FindRoadFile(int tile) { return Master.RoadFile.Roads.FirstOrDefault(fetch => fetch.Tile == tile); }
    }
}
