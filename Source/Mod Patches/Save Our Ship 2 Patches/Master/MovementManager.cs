using GameClient;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace RT_SOS2Patches.Master
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
        public static void MoveShipFromTile(MovementData data) 
        {
            WorldObjectFakeOrbitingShip ship = PlayerSpaceSettlementManager.spacePlayerSettlement.Find(x => x.Tile == data.tile);
            ship.phi = data.phi;
            ship.theta = data.theta;
            ship.radius = data.radius;
            ship.OrbitSet();
        }
    }
}
