using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class ShipMovementManager
    {
        public readonly static string fileExtension = ".mpsettlement";
        public static void HandlePacket(ServerClient client, Packet packet) 
        {
            MovementData data = Serializer.ConvertBytesToObject<MovementData>(packet.contents);
            UpdateShip(client, data);
        }

        public static void UpdateShip(ServerClient client, MovementData data)
        {
            SpaceSettlementFile file = (SpaceSettlementFile)SettlementManager.GetSettlementFileFromTile(data.tile);
            if (file != null)
            {
                if (file.owner == client.userFile.Username)
                {
                    file.phi = data.phi;
                    file.theta = data.theta;
                    file.radius = data.radius;
                    Serializer.SerializeToFile(Path.Combine(Master.settlementsPath, data.tile + fileExtension), file);
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ShipMovementPacket), new MovementData() { phi = file.phi, theta = file.theta, radius = file.radius, tile = file.tile });
                    foreach(ServerClient gameClient in Network.connectedClients) 
                    {
                        if(gameClient != client)gameClient.listener.EnqueuePacket(packet);
                    }
                    if (Master.serverConfig.ExtremeVerboseLogs)
                    {
                        Logger.Warning($"[SOS2]{file.owner}'s ship moved on tile {file.tile} with coordinate:\nPhi:{file.phi}, Theta:{file.theta}, Radius:{file.theta}");
                    }
                }
                else
                {
                    Logger.Warning($"[SOS2]{client.userFile.Username} tried to move {file.owner}'s ship at tile {file.tile}");
                }
            } else 
            {
                Logger.Warning($"[SOS2]Tried moving {client.userFile.Username}'s ship on tile {data.tile}, but it did not exist.");
            }
        }
    }
}
