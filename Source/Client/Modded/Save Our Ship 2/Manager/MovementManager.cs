using Shared;
using System.Threading;
using System.Threading.Tasks;

namespace GameClient.SOS2
{
    public static class MovementManager
    {
        public static bool shipMoved = false;
        public static float phi;
        public static float theta;
        public static float radius;
        public static int tile = -1;
        private static readonly int sleepTime = 125;
        static MovementManager() 
        {
            Task.Run(PositionChecker);
        }
        public static void PositionChecker() 
        {
            while (true)
            {
                Thread.Sleep(sleepTime);
                if (shipMoved)
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ShipMovementPacket), new MovementData() { phi = phi, theta = theta, radius = radius, tile = tile });
                    Network.listener.EnqueuePacket(packet);
                    shipMoved = false;
                }
            }

        }
        public static void MoveShipFromTile(Packet data) 
        {
            MovementData movement = Serializer.ConvertBytesToObject<MovementData>(data.contents);
            WorldObjectFakeOrbitingShip ship = PlayerShipManager.spacePlayerSettlement.Find(x => x.Tile == movement.tile);
            ship.phi = movement.phi;
            ship.theta = movement.theta;
            ship.radius = movement.radius;
            ship.OrbitSet();
        }
    }
}
